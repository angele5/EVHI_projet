using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BitalinoScript : MonoBehaviour
{
    // Class Variables
    private PluxDeviceManager pluxDevManager;

    // Class constants (CAN BE EDITED BY IN ACCORDANCE TO THE DESIRED DEVICE CONFIGURATIONS)
    [System.NonSerialized]
    public List<string> domains = new List<string>() { "BTH" };
    public string deviceMacAddress = "BTH20:18:08:08:02:30";
    public int samplingRate = 100;
    public int resolution = 10;
    private const double LOW_CUTOFF = 20.0; // Fréquence de coupure basse (Hz)
    private const double HIGH_CUTOFF = 500.0; // Fréquence de coupure haute (Hz)
    public PlayerModel playerModel;



    private bool isScanFinished = false;
    private bool isScanning = false;
    private bool isConnectionDone = false;
    private bool isConnecting = false; 
    private bool isAcquisitionStarted = false;

    private List<double> leftBuffer = new List<double>();
    private List<double> rightBuffer = new List<double>();
    private ButterworthFilter leftFilter;
    private ButterworthFilter rightFilter;

    private List<double> leftContractions = new List<double>();
    private List<double> rightContractions = new List<double>();
    private bool isLeftContraction = false;
    private bool isRightContraction = false;
    private List<double> leftContractionBuffer = new List<double>();
    private List<double> rightContractionBuffer = new List<double>();
    private bool addedLeft = false;
    private bool addedRight = false;


    // Start is called before the first frame update
    private void Start()
    {
        // Initialise object
        pluxDevManager = new PluxDeviceManager(ScanResults, ConnectionDone, AcquisitionStarted, OnDataReceived, OnEventDetected, OnExceptionRaised);

        // Important call for debug purposes by creating a log file in the root directory of the project.
        pluxDevManager.WelcomeFunctionUnity();

        // Create the Butterworth filters
        leftFilter = new ButterworthFilter(samplingRate, LOW_CUTOFF, HIGH_CUTOFF);
        rightFilter = new ButterworthFilter(samplingRate, LOW_CUTOFF, HIGH_CUTOFF);
        playerModel.isConnected = false;
    }

    // Update function, being constantly invoked by Unity.
    private void Update()
    { 
        if (isScanning || isConnecting || isAcquisitionStarted)
        {
            return;
        }

        if (!isScanFinished)
        {
            // Search for PLUX devices
            pluxDevManager.GetDetectableDevicesUnity(domains);
            isScanning = true;
            Debug.Log("Scanning for devices...");
            return;
        }


        if (!isConnectionDone)
        {
            // Connect to the device selected in the Dropdown list.
            pluxDevManager.PluxDev(deviceMacAddress);
            Debug.Log("Connecting to device " + deviceMacAddress);
            isConnecting = true;
            return;
        }

        if (!isAcquisitionStarted)
        {
            // Start the acquisition
            pluxDevManager.StartAcquisitionUnity(samplingRate, new List<int> {1,2,3,4 }, resolution);
            return;
        }

    }

    // Method invoked when the application was closed.
    private void OnApplicationQuit()
    {

        // Disconnect from device.
        if (pluxDevManager != null)
        {
            pluxDevManager.DisconnectPluxDev();
            Debug.Log("Application ending after " + Time.time + " seconds");
        }

    }

    /**
     * =================================================================================
     * ============================= GUI Events ========================================
     * =================================================================================
     */

    /**
     * =================================================================================
     * ============================= Callbacks =========================================
     * =================================================================================
     */

    // Callback that receives the list of PLUX devices found during the Bluetooth scan.
    public void ScanResults(List<string> listDevices)
    {

        if (listDevices.Count > 0)
        {

            isScanFinished = true;
            isScanning = false;
            // Show an informative message about the number of detected devices.
            Debug.Log("Bluetooth device scan found: " + listDevices[0]);
            // deviceMacAddress = listDevices[0];
        }
        else
        {
            // Show an informative message stating the none devices were found.
            Debug.Log("No devices were found. Please make sure the device is turned on and in range.");
            isScanning = false;
        }
    }

    // Callback invoked once the connection with a PLUX device was established.
    // connectionStatus -> A boolean flag stating if the connection was established with success (true) or not (false).
    public void ConnectionDone(bool connectionStatus)
    {
        if (connectionStatus)
        {
            isConnectionDone = true;
            isConnecting = false;
            Debug.Log("Connexion réussie à l'appareil BITalino");
        }
        else
        {
            Debug.Log("Erreur lors de la connexion à l'appareil");
            isConnecting = false;
        }
    }

    // Callback invoked once the data streaming between the PLUX device and the computer is started.
    // acquisitionStatus -> A boolean flag stating if the acquisition was started with success (true) or not (false).
    // exceptionRaised -> A boolean flag that identifies if an exception was raised and should be presented in the GUI (true) or not (false).
    public void AcquisitionStarted(bool acquisitionStatus, bool exceptionRaised = false, string exceptionMessage = "")
    {
        if (acquisitionStatus)
        {
            isAcquisitionStarted = true;
            Debug.Log("Acquisition démarrée avec succès");
        }
        else
        {
            Debug.Log("Erreur lors du démarrage de l'acquisition: " + exceptionMessage);
        }
    }

    // Callback invoked every time an exception is raised in the PLUX API Plugin.
    // exceptionCode -> ID number of the exception to be raised.
    // exceptionDescription -> Descriptive message about the exception.
    public void OnExceptionRaised(int exceptionCode, string exceptionDescription)
    {
        if (pluxDevManager.IsAcquisitionInProgress())
        {
            Debug.Log("Exception raised: " + exceptionDescription);
        }
    }

    private double CalculateRMS(List<double> signal)
    {
        double sum = 0;
        foreach (double value in signal)
        {
            sum += value * value;
        }

        return Math.Sqrt(sum / signal.Count);
    }

    public static bool DetectFatigue(List<double> emgValues, int windowSize = 5, double threshold = 0.05)
    {
        if (emgValues == null || emgValues.Count < windowSize)
            return false;

        List<double> movingAverages = new List<double>();

        // Calcul de la moyenne mobile
        for (int i = 0; i <= emgValues.Count - windowSize; i++)
        {
            double avg = emgValues.Skip(i).Take(windowSize).Average();
            movingAverages.Add(avg);
        }

        // Vérifier si la tendance est décroissante sur les dernières valeurs
        int decreasingCount = 0;
        for (int i = 1; i < movingAverages.Count; i++)
        {
            if (movingAverages[i] < movingAverages[i - 1] * (1 - threshold)) 
                decreasingCount++;
        }
        // Debug.Log("Decreasing count: " + decreasingCount + " Total count: " + movingAverages.Count);
        // Considérer la fatigue si une tendance à la baisse est détectée
        return decreasingCount > movingAverages.Count * 0.5; // x % des points doivent être en baisse
    }


    // Callback that receives the data acquired from the PLUX devices that are streaming real-time data.
    // nSeq -> Number of sequence identifying the number of the current package of data.
    // data -> Package of data containing the RAW data samples collected from each active channel ([sample_first_active_channel, sample_second_active_channel,...]).
    public void OnDataReceived(int nSeq, int[] data)
    {
        playerModel.isConnected = true;

        double channel1 = data[0]; // left quality
        double channel2 = data[1]; // Left arm
        double channel3 = data[2]; // right quality
        double channel4 = data[3]; // Right arm

        // Normalisation du signal en tension
        channel2 = (channel2 - 512) * (3.3 / 1023.0);
        channel4 = (channel4 - 512) * (3.3 / 1023.0);         

        // Filtrage passe-bande
        channel2 = leftFilter.Apply(channel2);
        channel4 = rightFilter.Apply(channel4);

        // Valeur absolue
        channel2 = Math.Abs(channel2);
        channel4 = Math.Abs(channel4);

        //Debug.Log("Left: " + channel2 + " Right: " + channel4);

        // Add the new value to the left buffer
        if (channel1 >= 0){
            if (leftBuffer.Count == 50){
                leftBuffer.RemoveAt(0);
            }
            leftBuffer.Add(channel2);
        } 

        // Add the new value to the right buffer
        if (channel3 >= 0){
            if (rightBuffer.Count == 50){
                rightBuffer.RemoveAt(0);
            }
            rightBuffer.Add(channel4);
        }

        double leftRMS = -10;
        if (leftBuffer.Count > 0)
        {
            leftRMS = CalculateRMS(leftBuffer);
        }

        double rightRMS = -10;
        if (rightBuffer.Count > 0)
        {
            rightRMS = CalculateRMS(rightBuffer);
        }

        // Update the player model
        // Debug.Log("Left RMS: " + leftRMS + " Right RMS: " + rightRMS + " BufferLeft: " + leftBuffer.Count + " BufferRight: " + rightBuffer.Count); 
        // If the leftAverage is over the leftThreshold, the player is moving to the left
        if (leftRMS > playerModel.leftThreshold)
        {
            playerModel.isLeft = true;
            leftContractionBuffer.Add(leftRMS); // Ajout de la data dans le buffer d'une contraction a gauche
            isLeftContraction = true;

        }else
        {
            playerModel.isLeft = false;
            if (isLeftContraction) // Fin de la contraction a gauche
            {
                if (leftContractionBuffer.Count > 50)
                {
                    leftContractions.Add(leftContractionBuffer.Average());
                }
                leftContractionBuffer.Clear();
                isLeftContraction = false;
                addedLeft = true; // Ajout de la contraction a gauche
            }
        }

        // If the rightAverage is over the rightThreshold, the player is moving to the right
        if (rightRMS > playerModel.rightThreshold)
        {
            playerModel.isRight = true;
            rightContractionBuffer.Add(rightRMS); // Ajout de la data dans le buffer d'une contraction a droite
            isRightContraction = true;
        }else
        {
            playerModel.isRight = false;
            if (isRightContraction) // Fin de la contraction a droite
            {
                if (rightContractionBuffer.Count > 50)
                {
                    rightContractions.Add(rightContractionBuffer.Average());
                }
                rightContractionBuffer.Clear();
                isRightContraction = false;
                addedRight = true; // Ajout de la contraction a droite
            }
        }

        if (addedLeft){
            addedLeft = false;
            bool fatigueLeft = DetectFatigue(leftContractions);
            if (fatigueLeft)
            {
                Debug.Log("Fatigue détectée à gauche");
            }

        }

        if (addedRight){
            addedRight = false;
            bool fatigueRight = DetectFatigue(rightContractions);
            if (fatigueRight)
            {
                Debug.Log("Fatigue détectée à droite");
            }

        }

        // Debug Nombre de contractions
        // Debug.Log("Left contractions: " + leftContractions.Count + " Right contractions: " + rightContractions.Count);
        // // Show the current package of data.
        // string outputString = "Acquired Data "+nSeq+":\n";
        // for (int j = 0; j < data.Length; j++)
        // {
        //     outputString += data[j] + "\t";
        // }

        // Show the values in the GUI.
        // Debug.Log(outputString);
    
    }

    // Callback that receives the events raised from the PLUX devices that are streaming real-time data.
    // pluxEvent -> Event object raised by the PLUX API.
    public void OnEventDetected(PluxDeviceManager.PluxEvent pluxEvent)
    {
        if (pluxEvent is PluxDeviceManager.PluxDisconnectEvent)
        {
            // Present an error message.
            Debug.Log("The device was disconnected. Please make sure the device is turned on and in range.");

            // Securely stop the real-time acquisition.
            pluxDevManager.StopAcquisitionUnity(-1);

        }
        else if (pluxEvent is PluxDeviceManager.PluxDigInUpdateEvent)
        {
            // PluxDeviceManager.PluxDigInUpdateEvent digInEvent = (pluxEvent as PluxDeviceManager.PluxDigInUpdateEvent);
            // Debug.Log("Digital Input Update Event Detected on channel " + digInEvent.channel + ". Current state: " + digInEvent.state);
        }
    }
}


// Classe pour un filtre Butterworth passe-bande
class ButterworthFilter
{
    private double[] a, b;
    private double[] x, y;

    public ButterworthFilter(int sampleRate, double lowCutoff, double highCutoff)
    {
        double nyquist = sampleRate / 2.0;
        double low = lowCutoff / nyquist;
        double high = highCutoff / nyquist;

        // Coefficients de filtre de Butterworth (ordre 2)
        b = new double[] { 0.2929, 0, -0.2929 };
        a = new double[] { 1, -0.5858, 0.1716 };

        x = new double[3];
        y = new double[3];
    }

    public double Apply(double input)
    {
        x[2] = x[1];
        x[1] = x[0];
        x[0] = input;

        y[2] = y[1];
        y[1] = y[0];

        y[0] = b[0] * x[0] + b[1] * x[1] + b[2] * x[2] - a[1] * y[1] - a[2] * y[2];

        return y[0];
    }
}