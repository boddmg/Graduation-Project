//------------------------------------------------------------------------------
// <copyright file="KinectSpeechCommander.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Resources;
    using System.Windows.Threading;
    using Microsoft.Kinect;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;

    public class SpeechCommanderEventArgs : EventArgs
    {
        public string Command { get; internal set; }

        public string Words { get; internal set; }

        public double ConfidenceLevel { get; internal set; }

        public bool ActiveModeRecognition { get; internal set; }
    }

    public class ActiveListeningModeChangedEventArgs : EventArgs
    {
        public bool ActiveListeningModeEnabled { get; internal set; }
    }

    /// <summary>
    /// This control is used to return speech recognition events
    /// It supports Active Listening mode; using a keyword to enable the recognition of specific commands
    /// Default Listening will simply recognize the text spoken by the user and can be prone to false activation.
    /// Use in conjunction with 2 grammars if you want both modes: one for the Defaul Listening and one for the Active Listening mode.
    /// </summary>
    public class KinectSpeechCommander : Control, IDisposable
    {
        /// <summary>
        /// Timeout to go back from Active Listening to Default Listening, in seconds.
        /// </summary>
        public static readonly DependencyProperty ActiveListeningTimeoutProperty =
            DependencyProperty.Register("ActiveListeningTimeout", typeof(int), typeof(KinectSpeechCommander), new FrameworkPropertyMetadata(20, FrameworkPropertyMetadataOptions.None));

        /// <summary>
        /// Path to Default Listening Grammar.
        /// </summary>
        public static readonly DependencyProperty DefaultListeningGrammarUriProperty =
            DependencyProperty.Register("DefaultListeningGrammarUri", typeof(Uri), typeof(KinectSpeechCommander), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        /// <summary>
        /// Name of rule that defines Default Listening Grammar. May be <code>null</code>.
        /// </summary>
        public static readonly DependencyProperty DefaultListeningGrammarRuleProperty =
            DependencyProperty.Register("DefaultListeningGrammarRule", typeof(string), typeof(KinectSpeechCommander), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        /// <summary>
        /// Path to Active Listening Grammar.
        /// </summary>
        public static readonly DependencyProperty ActiveListeningGrammarUriProperty =
            DependencyProperty.Register("ActiveListeningGrammarUri", typeof(Uri), typeof(KinectSpeechCommander), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        /// <summary>
        /// Name of rule that defines Active Listening Grammar. May be <code>null</code>.
        /// </summary>
        public static readonly DependencyProperty ActiveListeningGrammarRuleProperty =
            DependencyProperty.Register("ActiveListeningGrammarRule", typeof(string), typeof(KinectSpeechCommander), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        /// <summary>
        /// Default confidence Threshold for recognition.
        /// </summary>
        public static readonly DependencyProperty DefaultConfidenceThresholdProperty =
            DependencyProperty.Register("DefaultConfidenceThreshold", typeof(float), typeof(KinectSpeechCommander), new FrameworkPropertyMetadata(.75f, FrameworkPropertyMetadataOptions.None));

        /// <summary>
        /// <code>true</code> if KinectAudioSource has been started, <code>false</code> otherwise.
        /// </summary>
        private bool started = false;

        /// <summary>
        /// KinectSensor being used.
        /// </summary>
        private KinectSensor kinect = null;

        /// <summary>
        /// <code>true</code> if KinectSpeechCommander is in active listening mode
        /// (i.e.: user has said "kinect" keyword).
        /// <code>false</code> otherwise.
        /// </summary>
        private bool activeListeningMode = false;

        /// <summary>
        /// Engine used for speech recognition.
        /// </summary>
        private SpeechRecognitionEngine speechRecognitionEngine = null;

        /// <summary>
        /// Timer used to determine if we should get out of active listening mode,
        /// i.e., user hasn't uttered a command since entering active mode.
        /// </summary>
        private DispatcherTimer activeListeningTimer = new DispatcherTimer();

        /// <summary>
        /// Grammar used while in active listening mode.
        /// </summary>
        private Grammar activeListeningGrammar = null;

        /// <summary>
        /// Grammar used while in default listening mode.
        /// </summary>
        private Grammar defaultListeningGrammar = null;

        /// <summary>
        /// Item recognized.
        /// </summary>
        public event EventHandler<SpeechCommanderEventArgs> SpeechRecognized;

        /// <summary>
        /// Item not recognized.
        /// </summary>
        public event EventHandler<SpeechCommanderEventArgs> SpeechRejected;

        /// <summary>
        /// Active Listening Mode Changed.
        /// </summary>
        public event EventHandler<ActiveListeningModeChangedEventArgs> ActiveListeningModeChanged;

        /// <summary>
        /// Gets or sets the timeout to go back from Active Listening to Default Listening, in seconds.
        /// </summary>
        public int ActiveListeningTimeout
        {
            get { return (int)GetValue(ActiveListeningTimeoutProperty); }
            set { SetValue(ActiveListeningTimeoutProperty, value); }
        }

        /// <summary>
        /// Gets or sets the path to Default Listening Grammar
        /// </summary>
        public Uri DefaultListeningGrammarUri
        {
            get { return (Uri)GetValue(DefaultListeningGrammarUriProperty); }
            set { SetValue(DefaultListeningGrammarUriProperty, value); }
        }

        /// <summary>
        /// Gets or sets the name of rule that defines Default Listening Grammar. May be <code>null</code>.
        /// </summary>
        public string DefaultListeningGrammarRule
        {
            get { return (string)GetValue(DefaultListeningGrammarRuleProperty); }
            set { SetValue(DefaultListeningGrammarRuleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the path to Active Listening Grammar.
        /// </summary>
        public Uri ActiveListeningGrammarUri
        {
            get { return (Uri)GetValue(ActiveListeningGrammarUriProperty); }
            set { SetValue(ActiveListeningGrammarUriProperty, value); }
        }

        /// <summary>
        /// Gets or sets the name of the rule that defines Active Listening Grammar. May be <code>null</code>.
        /// </summary>
        public string ActiveListeningGrammarRule
        {
            get { return (string)GetValue(ActiveListeningGrammarRuleProperty); }
            set { SetValue(ActiveListeningGrammarRuleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the default confidence Threshold for recognition
        /// </summary>
        public float DefaultConfidenceThreshold
        {
            get { return (float)GetValue(DefaultConfidenceThresholdProperty); }
            set { SetValue(DefaultConfidenceThresholdProperty, value); }
        }

        /// <summary>
        /// Start loads the grammars, connects to speech engine to the Kinect audio stream
        /// and starts asynchronous speech recognition.
        /// </summary>
        /// <param name="sensor">
        /// KinectSensor that will provide audio stream for which speech will be recognized.
        /// </param>
        public void Start(KinectSensor sensor)
        {
            if (this.started)
            {
                return;
            }

            // Load Grammars from files
            this.defaultListeningGrammar = CreateGrammar(this.DefaultListeningGrammarUri, this.DefaultListeningGrammarRule);
            this.activeListeningGrammar = CreateGrammar(this.ActiveListeningGrammarUri, this.ActiveListeningGrammarRule);

            // If we couldn't find either of the grammars, we can't do any work, so we bail out
            if (this.defaultListeningGrammar == null || this.activeListeningGrammar == null)
            {
                return;
            }

            this.kinect = sensor;
            this.activeListeningTimer = new DispatcherTimer();

            RecognizerInfo recognizerInfo = GetKinectRecognizer();
            this.speechRecognitionEngine = new SpeechRecognitionEngine(recognizerInfo.Id);

            // Load the Grammars in the speech recognition engine
            this.speechRecognitionEngine.LoadGrammar(this.defaultListeningGrammar);
            this.speechRecognitionEngine.LoadGrammar(this.activeListeningGrammar);

            this.EnableGrammars();

            // Hook - up speech recognition events.
            this.speechRecognitionEngine.SpeechRecognized += this.Sre_SpeechRecognized;
            this.speechRecognitionEngine.SpeechRecognitionRejected += this.Sre_SpeechRejected;

            // Diagnostic event to tune up the kinect mic volume
            this.speechRecognitionEngine.AudioSignalProblemOccurred += this.Sre_AudioSignalProblemOccurred;

            // Recognize now...
            this.speechRecognitionEngine.SetInputToAudioStream(this.kinect.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            this.speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

            this.started = true;
        }

        /// <summary>
        /// Stops the speech recognition engine and Kinect audio streaming.
        /// </summary>
        public void Stop()
        {
            if (!this.started)
            {
                return;
            }

            if (this.kinect != null && this.kinect.AudioSource != null)
            {
                this.kinect.AudioSource.Stop();
            }

            if (this.speechRecognitionEngine != null)
            {
                this.speechRecognitionEngine.RecognizeAsyncCancel();
                this.speechRecognitionEngine.RecognizeAsyncStop();
            }

            this.started = false;
        }

        /// <summary>
        /// Stops speech recognition, if necessary, and disposes all resources used by the
        /// <see cref="KinectSpeechCommander"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Stops speech recognition, if necessary, and disposes all resources used by the
        /// <see cref="KinectSpeechCommander"/>.
        /// </summary>
        /// <param name="disposing">
        /// Specify true to indicate that the class should clean up all resources,
        /// including native resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.speechRecognitionEngine != null)
                {
                    this.speechRecognitionEngine.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets a speech recognizer appropriate for Kinect audio stream.
        /// </summary>
        /// <returns>
        /// <see cref="RecognizerInfo"/> that describes an appropriate speech recognizer.
        /// <code>null</code> if no such recognizer was found.
        /// </returns>
        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.OrdinalIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        /// <summary>
        /// Create a speech recognition grammar from the specified grammar path
        /// and rule name.
        /// </summary>
        /// <param name="grammarUri">
        /// Path to grammar specification to use as source for created grammar.
        /// </param>
        /// <param name="grammarRule">
        /// Name of rule within specification that defines grammar to use. May be <code>null</code>.
        /// </param>
        /// <returns>
        /// A grammar ready to be loaded into a speech recognition engine.
        /// <code>null</code> if grammar specification was not found.
        /// </returns>
        /// <remarks>
        /// This method first attempts to find a grammar file that matches the
        /// specified Uri, and if that fails then attempts to find a resource
        /// that matches the Uri.
        /// </remarks>
        private static Grammar CreateGrammar(Uri grammarUri, string grammarRule)
        {
            if (File.Exists(grammarUri.ToString()))
            {
                // If we're dealing with a file name, create grammar from filename
                return new Grammar(grammarUri.ToString(), grammarRule);
            }
            else
            {
                StreamResourceInfo info = Application.GetResourceStream(grammarUri);
                if (info != null)
                {
                    // If we're dealing with a resource, create grammar from stream
                    return new Grammar(info.Stream, grammarRule);
                }
            }

            return null;
        }

        /// <summary>
        /// Helper to handle event dispatch for rejected speech utterances.
        /// </summary>
        /// <param name="command">
        /// Text of speech command that was rejected. May be <code>null</code>.
        /// </param>
        /// <param name="confidenceLevel">
        /// Value (from 0.0 to 1.0) assigned by the recognizer that represents the likelihood that a
        /// recognized phrase matches a given input.
        /// </param>
        /// <param name="activeModeRecognition">
        /// <code>true</code> if speech commander was in active listening mode when this event came in.
        /// <code>false</code> otherwise.
        /// </param>
        private void OnSpeechRejected(string command, double confidenceLevel, bool activeModeRecognition)
        {
            if (this.SpeechRejected != null)
            {
                SpeechCommanderEventArgs e = new SpeechCommanderEventArgs();
                e.Command = command;
                e.ConfidenceLevel = confidenceLevel;
                e.ActiveModeRecognition = activeModeRecognition;

                this.SpeechRejected(this, e);
            }
        }

        /// <summary>
        /// Helper to handle event dispatch for recognized speech utterances.
        /// </summary>
        /// <param name="command">
        /// Text of semantic speech command that was recognized.
        /// </param>
        /// <param name="words">
        /// Text of speech utterance that was recognized.
        /// </param>
        /// <param name="confidenceLevel">
        /// Value (from 0.0 to 1.0) assigned by the recognizer that represents the likelihood that a
        /// recognized phrase matches a given input.
        /// </param>
        /// <param name="activeModeRecognition">
        /// <code>true</code> if speech commander was in active listening mode when this event came in.
        /// <code>false</code> otherwise.
        /// </param>
        private void OnSpeechRecognized(string command, string words, double confidenceLevel, bool activeModeRecognition)
        {
            SpeechCommanderEventArgs e = new SpeechCommanderEventArgs();
            e.Command = command;
            e.Words = words;
            e.ConfidenceLevel = confidenceLevel;
            e.ActiveModeRecognition = activeModeRecognition;

            if (this.SpeechRecognized != null)
            {
                this.SpeechRecognized(this, e);
            }
        }

        /// <summary>
        /// Helper to handle event dispatch when active listening mode has changed.
        /// </summary>
        private void OnActiveListeningModeChanged()
        {
            if (this.ActiveListeningModeChanged != null)
            {
                ActiveListeningModeChangedEventArgs e = new ActiveListeningModeChangedEventArgs();
                e.ActiveListeningModeEnabled = this.activeListeningMode;

                this.ActiveListeningModeChanged(this, e);
            }
        }

        /// <summary>
        /// Handler for audio signal problem events.
        /// </summary>
        /// <param name="sender">
        /// Object that triggered the event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void Sre_AudioSignalProblemOccurred(object sender, AudioSignalProblemOccurredEventArgs e)
        {
            Debug.Print("Audio signal problem occurred: {0}", e.AudioSignalProblem.ToString());
        }

        /// <summary>
        /// Handler for speech recognized events.
        /// </summary>
        /// <param name="sender">
        /// Object that triggered the event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > this.DefaultConfidenceThreshold)
            {
                Debug.Print("Speech Recognized: {0} confidence: {1}", e.Result.Semantics.Value.ToString(), e.Result.Confidence);
                this.OnSpeechRecognized(e.Result.Semantics.Value.ToString(), e.Result.Text, e.Result.Confidence, this.activeListeningMode);

                // If the keyword is used, switch to Active Listening Mode
                if (e.Result.Semantics.Value.ToString() == "KEYWORD")
                {
                    this.SetActiveListeningModeOn();
                }
                else
                {
                    this.SetActiveListeningModeOff();
                }
            }
            else
            {
                Debug.Print("Speech Rejected by confidence: {0} confidence: {1}", e.Result.Text, e.Result.Confidence);
                this.OnSpeechRejected(e.Result.Text, e.Result.Confidence, this.activeListeningMode);
            }
        }

        /// <summary>
        /// Handler for speech rejected events.
        /// </summary>
        /// <param name="sender">
        /// Object that triggered the event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void Sre_SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Debug.Print("Speech Rejected by engine: {0} confidence: {1}", e.Result.Text, e.Result.Confidence);
            this.OnSpeechRejected(e.Result.Text, e.Result.Confidence, this.activeListeningMode);
        }

        /// <summary>
        /// Turn active Listening Mode On.
        /// </summary>
        private void SetActiveListeningModeOn()
        {
            if (this.activeListeningMode)
            {
                return;
            }

            this.activeListeningMode = true;
            this.OnActiveListeningModeChanged();
            this.EnableGrammars();
            this.StartActiveListeningTimer();
        }

        /// <summary>
        /// Turn active Listening Mode Off.
        /// </summary>
        private void SetActiveListeningModeOff()
        {
            if (!this.activeListeningMode)
            {
                return;
            }

            this.activeListeningMode = false;

            // Stop the timer, it's no longer needed.
            this.activeListeningTimer.Stop();

            this.OnActiveListeningModeChanged();
            this.EnableGrammars();
        }

        /// <summary>
        /// Start the active listening timer, to determine if mode should be automatically
        /// turned off.
        /// </summary>
        private void StartActiveListeningTimer()
        {
            this.activeListeningTimer.Stop();
            this.activeListeningTimer.Tick += this.ActiveListeningTimerTick;
            this.activeListeningTimer.Interval = new TimeSpan(0, 0, this.ActiveListeningTimeout);
            this.activeListeningTimer.Start();
        }

        /// <summary>
        /// Handler for active listening timer tick event.
        /// </summary>
        /// <param name="sender">
        /// Object that triggered the event.
        /// </param>
        /// <param name="e">
        /// Event arguments.
        /// </param>
        private void ActiveListeningTimerTick(object sender, EventArgs e)
        {
            this.SetActiveListeningModeOff();
        }

        /// <summary>
        /// Enable Grammars based on the current listening Mode (active vs default).
        /// </summary>
        private void EnableGrammars()
        {
            if (this.defaultListeningGrammar != null)
            {
                this.defaultListeningGrammar.Enabled = !this.activeListeningMode;
            }

            if (this.activeListeningGrammar != null)
            {
                this.activeListeningGrammar.Enabled = this.activeListeningMode;
            }
        }
    }
}
