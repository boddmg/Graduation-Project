// -----------------------------------------------------------------------
// <copyright file="KinectSkeletonChooser.cs" company="Microsoft IT">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Microsoft.Kinect;
    
    public enum SkeletonChooserMode
    {
        /// <summary>
        /// Use system default tracking
        /// </summary>
        DefaultSystemTracking,

        /// <summary>
        /// Track the player nearest to the sensor
        /// </summary>
        Closest1Player,

        /// <summary>
        /// Track the two players nearest to the sensor
        /// </summary>
        Closest2Player,

        /// <summary>
        /// Track one player based on id
        /// </summary>
        Sticky1Player,

        /// <summary>
        /// Track two players based on id
        /// </summary>
        Sticky2Player,

        /// <summary>
        /// Track one player based on most activity
        /// </summary>
        MostActive1Player,

        /// <summary>
        /// Track two players based on most activity
        /// </summary>
        MostActive2Player
    }

    /// <summary>
    /// KinectSkeletonChooser class is a lookless control that will select skeletons based on a specified heuristic.
    /// It contains logic to track players over multiple frames and use this data to select based on distance, activity level or other methods.
    /// It is intended that if you use this control, no other code will manage the skeleton tracking state on the Kinect Sensor,
    /// as they will collide in unpredictable ways.
    /// </summary>
    public class KinectSkeletonChooser : KinectControl
    {
        public static readonly DependencyProperty SkeletonChooserModeProperty =
            DependencyProperty.Register(
                "SkeletonChooserMode",
                typeof(SkeletonChooserMode),
                typeof(KinectSkeletonChooser),
                new PropertyMetadata(SkeletonChooserMode.DefaultSystemTracking, (sender, args) => ((KinectSkeletonChooser)sender).EnsureSkeletonStreamState()));

        private readonly List<ActivityWatcher> recentActivity = new List<ActivityWatcher>();
        private readonly List<int> activeList = new List<int>();
        private Skeleton[] skeletonData;

        public SkeletonChooserMode SkeletonChooserMode
        {
            get { return (SkeletonChooserMode)GetValue(KinectSkeletonChooser.SkeletonChooserModeProperty); }
            set { SetValue(KinectSkeletonChooser.SkeletonChooserModeProperty, value); }
        }

        protected override void OnKinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null == args)
            {
                throw new ArgumentNullException("args");
            }

            var oldKinectSensor = args.OldValue;
            var newKinectSensor = args.NewValue;

            if (oldKinectSensor != null)
            {
                oldKinectSensor.SkeletonFrameReady -= this.SkeletonFrameReady;
            }

            if (newKinectSensor != null && newKinectSensor.Status == KinectStatus.Connected)
            {
                newKinectSensor.SkeletonFrameReady += this.SkeletonFrameReady;
            }

            this.EnsureSkeletonStreamState();
        }

        private void EnsureSkeletonStreamState()
        {
            if ((null != this.KinectSensorManager) && (null != this.KinectSensorManager.KinectSensor))
            {
                this.KinectSensorManager.KinectSensor.SkeletonStream.AppChoosesSkeletons = SkeletonChooserMode.DefaultSystemTracking != this.SkeletonChooserMode;
            }
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            bool haveSkeletonData = false;

            // Ensure that this event still corresponds to the current Kinect Sensor (if any)
            if ((null == this.KinectSensorManager) || (!object.ReferenceEquals(this.KinectSensorManager.KinectSensor, sender)))
            {
                return;
            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                    haveSkeletonData = true;
                }
            }

            if (haveSkeletonData)
            {
                this.ChooseTrackedSkeletons(this.skeletonData);
            }
        }

        private void ChooseTrackedSkeletons(IEnumerable<Skeleton> skeletonDataValue)
        {
            switch (SkeletonChooserMode)
            {
                case SkeletonChooserMode.Closest1Player:
                    this.ChooseClosestSkeletons(skeletonDataValue, 1);
                    break;
                case SkeletonChooserMode.Closest2Player:
                    this.ChooseClosestSkeletons(skeletonDataValue, 2);
                    break;
                case SkeletonChooserMode.Sticky1Player:
                    this.ChooseOldestSkeletons(skeletonDataValue, 1);
                    break;
                case SkeletonChooserMode.Sticky2Player:
                    this.ChooseOldestSkeletons(skeletonDataValue, 2);
                    break;
                case SkeletonChooserMode.MostActive1Player:
                    this.ChooseMostActiveSkeletons(skeletonDataValue, 1);
                    break;
                case SkeletonChooserMode.MostActive2Player:
                    this.ChooseMostActiveSkeletons(skeletonDataValue, 2);
                    break;
            }
        }

        private void ChooseClosestSkeletons(IEnumerable<Skeleton> skeletonDataValue, int count)
        {
            SortedList<float, int> depthSorted = new SortedList<float, int>();

            foreach (Skeleton s in skeletonDataValue)
            {
                if (s.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    float valueZ = s.Position.Z;
                    while (depthSorted.ContainsKey(valueZ))
                    {
                        valueZ += 0.0001f;
                    }

                    depthSorted.Add(valueZ, s.TrackingId);
                }
            }

            this.ChooseSkeletonsFromList(depthSorted.Values, count);
        }

        private void ChooseOldestSkeletons(IEnumerable<Skeleton> skeletonDataValue, int count)
        {
            List<int> newList = (from s in skeletonDataValue where s.TrackingState != SkeletonTrackingState.NotTracked select s.TrackingId).ToList();

            // Remove all elements from the active list that are not currently present
            this.activeList.RemoveAll(k => !newList.Contains(k));

            // Add all elements that aren't already in the activeList
            this.activeList.AddRange(newList.FindAll(k => !this.activeList.Contains(k)));

            this.ChooseSkeletonsFromList(this.activeList, count);
        }

        private void ChooseMostActiveSkeletons(IEnumerable<Skeleton> skeletonDataValue, int count)
        {
            foreach (ActivityWatcher watcher in this.recentActivity)
            {
                watcher.NewPass();
            }

            foreach (Skeleton s in skeletonDataValue)
            {
                if (s.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    ActivityWatcher watcher = this.recentActivity.Find(w => w.TrackingId == s.TrackingId);
                    if (watcher != null)
                    {
                        watcher.Update(s);
                    }
                    else
                    {
                        this.recentActivity.Add(new ActivityWatcher(s));
                    }
                }
            }

            // Remove any skeletons that are gone
            this.recentActivity.RemoveAll(aw => !aw.Updated);

            this.recentActivity.Sort();
            this.ChooseSkeletonsFromList(this.recentActivity.ConvertAll(f => f.TrackingId), count);
        }

        private void ChooseSkeletonsFromList(IList<int> list, int max)
        {
            if (this.KinectSensorManager.SkeletonStreamEnabled)
            {
                int argCount = Math.Min(list.Count, max);

                if (argCount == 0)
                {
                    this.KinectSensorManager.KinectSensor.SkeletonStream.ChooseSkeletons();
                }

                if (argCount == 1)
                {
                    this.KinectSensorManager.KinectSensor.SkeletonStream.ChooseSkeletons(list[0]);
                }

                if (argCount >= 2)
                {
                    this.KinectSensorManager.KinectSensor.SkeletonStream.ChooseSkeletons(list[0], list[1]);
                }
            }
        }

        /// <summary>
        /// Private class used to track the activity of a given player over time, which can be used to assist the KinectSkeletonChooser 
        /// when determing which player to track.
        /// </summary>
        private class ActivityWatcher : IComparable<ActivityWatcher>
        {
            private const float ActivityFalloff = 0.98f;
            private float activityLevel;
            private SkeletonPoint previousPosition;
            private SkeletonPoint previousDelta;

            internal ActivityWatcher(Skeleton s)
            {
                this.activityLevel = 0.0f;
                this.TrackingId = s.TrackingId;
                this.Updated = true;
                this.previousPosition = s.Position;
                this.previousDelta = new SkeletonPoint();
            }

            internal int TrackingId { get; private set; }

            internal bool Updated { get; private set; }

            public int CompareTo(ActivityWatcher other)
            {
                if (null == other)
                {
                    return -1;
                }

                // Use the existing CompareTo on float, but reverse the arguments,
                // since we wish to have larger activityLevels sort ahead of smaller values.
                return other.activityLevel.CompareTo(this.activityLevel);
            }

            internal void NewPass()
            {
                this.Updated = false;
            }

            internal void Update(Skeleton s)
            {
                SkeletonPoint newPosition = s.Position;
                SkeletonPoint newDelta = new SkeletonPoint
                {
                    X = newPosition.X - this.previousPosition.X,
                    Y = newPosition.Y - this.previousPosition.Y,
                    Z = newPosition.Z - this.previousPosition.Z
                };

                SkeletonPoint deltaV = new SkeletonPoint
                {
                    X = newDelta.X - this.previousDelta.X,
                    Y = newDelta.Y - this.previousDelta.Y,
                    Z = newDelta.Z - this.previousDelta.Z
                };

                this.previousPosition = newPosition;
                this.previousDelta = newDelta;

                float deltaVLengthSquared = (deltaV.X * deltaV.X) + (deltaV.Y * deltaV.Y) + (deltaV.Z * deltaV.Z);
                float deltaVLength = (float)Math.Sqrt(deltaVLengthSquared);

                this.activityLevel = this.activityLevel * ActivityFalloff;
                this.activityLevel += deltaVLength;

                this.Updated = true;
            }
        }
    }
}
