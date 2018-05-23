using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 9/10/2015
// Purpose: FFAction is a robust ActionSystem using
//      inline Landa's to access variables in C#. It
//      is recommended that any variables which are
//      modified using Property be on the same object
//      as the FFAction component to reduce potential
//      null-exceptions of referencing destroyed objects.
//
// Usage: FFActionSystem can basically do anything from
//      AI programing to cutscene transitions... Anything
//      which is happening over time should utilize this
//      tool.
//
// Notes: FFActionSystem also can be used for predictive
//      movement in that you can timeWarp along a sequence
//      from a set time in the past or ahead. Using sequences
//      also makes pausing very easy as any sequence can be
//      paused and resume without any cost. See ExEnemy1/2/3
//      for examples
//
///////////////////////////////////////////////////////

public enum FFEase
{
    // Property ease (Choose one)
    E_Continuous = 0, // default
    E_SmoothStart = 2,
    E_SmoothEnd = 4,
    E_SmoothStartEnd = 8,
}

public class FFAction : MonoBehaviour
{
    // Interface and Data holder for Sequences
    public class ActionSequence
    {
        /// <summary>
        /// FFAction should be the only one to call this. Use FFAction.Sequence to get a ActionSequence
        /// </summary>
        public ActionSequence(FFAction actionSequence)
        {
            seqTime = FFSystem.time;
            _actionSequence = actionSequence;
            _lastClearSequenceTimeWarp = seqTime;
            _sequence.Add(new FFActionSet());
        }

        #region SequenceData
        /// <summary>
        /// In seconds, for warping
        /// </summary>
        public double seqTime;
        private double _lastClearSequenceTimeWarp;
        private FFAction _actionSequence;
        private List<FFActionSet> _sequence = new List<FFActionSet>();
        public List<FFActionSet> seqData
        {
            get { return _sequence; }
        }

        private bool _active = true;
        public bool affectedByTimeScale = true;
        public bool Active
        {
            get { return _active; }
        }
        #endregion

        /// <summary>
        /// Clears the sequence of all its actions.
        /// </summary>
        public void ClearSequence()
        {
            _sequence.Clear();
            _sequence.Add(new FFActionSet());
        }

        public bool IsEmpty()
        {
            return _sequence.Count == 0;
        }
        public bool IsComplete()
        {
            var seq = _sequence;
            if(_sequence.Count == 1)
            {
                bool incomplete = false;
                
                incomplete |= seq[0].as_VoidCalls         != null ? seq[0].as_VoidCalls        .Count != 0   : false;
                incomplete |= seq[0].as_ObjectCalls       != null ? seq[0].as_ObjectCalls      .Count != 0   : false;
                incomplete |= seq[0].as_intProperties     != null ? seq[0].as_intProperties    .Count != 0   : false;
                incomplete |= seq[0].as_floatProperties   != null ? seq[0].as_floatProperties  .Count != 0   : false;
                incomplete |= seq[0].as_Vector2Properties != null ? seq[0].as_Vector2Properties.Count != 0   : false;
                incomplete |= seq[0].as_Vector3Properties != null ? seq[0].as_Vector3Properties.Count != 0   : false;
                incomplete |= seq[0].as_Vector4Properties != null ? seq[0].as_Vector4Properties.Count != 0   : false;
                incomplete |= seq[0].as_ColorProperties   != null ? seq[0].as_ColorProperties  .Count != 0   : false;
                incomplete |= seq[0].as_DelayTime > 0.0f;

                return incomplete == false;
            }
            return false;
        }
        // All actions must complete to move to the next set of actions
        public void Sync()
        {
            _sequence.Add(new FFActionSet());
        }
        // @TODO @Maybe: Add FFRef<Boolean> with Sync for syning to a boolean start across multi actions

        #region TimeControl
        public void Pause()
        {
            _active = false;
        }

        public void Resume()
        {
            _active = true;
            seqTime = FFSystem.time;
            _actionSequence.Update();
        }

        public void ResumeTimeWarpFromPause()
        {
            _active = true;
            _actionSequence.Update();
        }

        /// <summary>
        /// returns true if the time given was past the last time warp in
        /// the sequence and not after the current time. The sequence will
        /// then catchup in the sequence to the current time from the given
        /// time in this function.
        /// </summary>
        public bool TimeWarpFrom(double time)
        {
            if (time >= _lastClearSequenceTimeWarp && time <= FFSystem.time)
            {
                seqTime = time;
                _actionSequence.Update();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Warps ahead in the sequence a set amount of time.
        /// </summary>
        /// <param name="time"></param>
        public void TimeWarpAhead(double time)
        {
            seqTime -= time;
            _actionSequence.Update();
        }
        #endregion

        #region Delay
        // An action which does nothing but wait
        public void Delay(float time)
        {
            if(_sequence.Count != 0)
                _sequence[_sequence.Count - 1].as_DelayTime += time;    
        }
        public void Delay(double time)
        {
            if (_sequence.Count != 0)
                _sequence[_sequence.Count - 1].as_DelayTime += (float)time;   
        }
        #endregion

        #region Calls
        public void Call(FFActionObjectCall fun, object obj)
        {
            if (_sequence.Count != 0 && fun != null && obj != null)
            {
                if (_sequence[_sequence.Count - 1].as_ObjectCalls == null)
                {
                    _sequence[_sequence.Count - 1].as_ObjectCalls = new Queue<FFActionObjectCaller>();
                }
                _sequence[_sequence.Count - 1].as_ObjectCalls.Enqueue(new FFActionObjectCaller(fun, obj));
            }
            else
            {
                Debug.Log("Error in ActionSequence Call call");
            }
        }

        public void Call(FFActionVoidCall fun)
        {
            if(_sequence.Count != 0 && fun != null)
            {
                if(_sequence[_sequence.Count - 1].as_VoidCalls == null)
                {
                    _sequence[_sequence.Count - 1].as_VoidCalls = new Queue<FFActionVoidCall>();
                }
                _sequence[_sequence.Count - 1].as_VoidCalls.Enqueue(fun);
            }
            else
            {
                Debug.Log("Error in ActionSequence Call call");
            }
        }
        #endregion

        #region properties
        public void Property(FFRef<int> var, int endValue, FFEase easeType, float timeToComplete)
        {
            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<int> myprop = new FFActionProperty<int>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<int>(myprop, easeType);

                // Add to front of sequence
                if(_sequence[_sequence.Count - 1].as_intProperties == null)
                {
                    _sequence[_sequence.Count - 1].as_intProperties = new List<FFActionProperty<int>>();
                }
                _sequence[_sequence.Count - 1].as_intProperties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<int> var, int endValue, AnimationCurve curve, float timeToComplete = CurveTime)
        {
            if (timeToComplete == CurveTime)
                timeToComplete = curve.TimeToComplete();

            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<int> myprop = new FFActionProperty<int>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<int>(myprop, curve);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_intProperties == null)
                {
                    _sequence[_sequence.Count - 1].as_intProperties = new List<FFActionProperty<int>>();
                }
                _sequence[_sequence.Count - 1].as_intProperties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<float> var, float endValue, FFEase easeType, float timeToComplete)
        {
            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<float> myprop = new FFActionProperty<float>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);
                
                SetMuGetter<float>(myprop, easeType);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_floatProperties == null)
                {
                    _sequence[_sequence.Count - 1].as_floatProperties = new List<FFActionProperty<float>>();
                }
                _sequence[_sequence.Count - 1].as_floatProperties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<float> var, float endValue, AnimationCurve curve, float timeToComplete = CurveTime)
        {
            if (timeToComplete == CurveTime)
                timeToComplete = curve.TimeToComplete();

            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<float> myprop = new FFActionProperty<float>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<float>(myprop, curve);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_floatProperties == null)
                {
                    _sequence[_sequence.Count - 1].as_floatProperties = new List<FFActionProperty<float>>();
                }
                _sequence[_sequence.Count - 1].as_floatProperties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<Vector2> var, Vector2 endValue, FFEase easeType, float timeToComplete)
        {
            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<Vector2> myprop = new FFActionProperty<Vector2>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.prev_value = var.Val;
                myprop.curr_time = 0.0f;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<Vector2>(myprop, easeType);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_Vector2Properties == null)
                {
                    _sequence[_sequence.Count - 1].as_Vector2Properties = new List<FFActionProperty<Vector2>>();
                }
                _sequence[_sequence.Count - 1].as_Vector2Properties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<Vector2> var, Vector2 endValue, AnimationCurve curve, float timeToComplete = CurveTime)
        {
            if (timeToComplete == CurveTime)
                timeToComplete = curve.TimeToComplete();

            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<Vector2> myprop = new FFActionProperty<Vector2>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.prev_value = var.Val;
                myprop.curr_time = 0.0f;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<Vector2>(myprop, curve);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_Vector2Properties == null)
                {
                    _sequence[_sequence.Count - 1].as_Vector2Properties = new List<FFActionProperty<Vector2>>();
                }
                _sequence[_sequence.Count - 1].as_Vector2Properties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<Vector3> var, Vector3 endValue, FFEase easeType, float timeToComplete)
        {
            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<Vector3> myprop = new FFActionProperty<Vector3>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<Vector3>(myprop, easeType);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_Vector3Properties == null)
                {
                    _sequence[_sequence.Count - 1].as_Vector3Properties = new List<FFActionProperty<Vector3>>();
                }
                _sequence[_sequence.Count - 1].as_Vector3Properties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<Vector3> var, Vector3 endValue, AnimationCurve curve, float timeToComplete = CurveTime)
        {
            if (timeToComplete == CurveTime)
                timeToComplete = curve.TimeToComplete();

            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<Vector3> myprop = new FFActionProperty<Vector3>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<Vector3>(myprop, curve);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_Vector3Properties == null)
                {
                    _sequence[_sequence.Count - 1].as_Vector3Properties = new List<FFActionProperty<Vector3>>();
                }
                _sequence[_sequence.Count - 1].as_Vector3Properties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<Vector4> var, Vector4 endValue, FFEase easeType, float timeToComplete)
        {
            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<Vector4> myprop = new FFActionProperty<Vector4>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<Vector4>(myprop, easeType);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_Vector4Properties == null)
                {
                    _sequence[_sequence.Count - 1].as_Vector4Properties = new List<FFActionProperty<Vector4>>();
                }
                _sequence[_sequence.Count - 1].as_Vector4Properties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<Vector4> var, Vector4 endValue, AnimationCurve curve, float timeToComplete = CurveTime)
        {
            if (timeToComplete == CurveTime)
                timeToComplete = curve.TimeToComplete();

            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<Vector4> myprop = new FFActionProperty<Vector4>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<Vector4>(myprop, curve);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_Vector4Properties == null)
                {
                    _sequence[_sequence.Count - 1].as_Vector4Properties = new List<FFActionProperty<Vector4>>();
                }
                _sequence[_sequence.Count - 1].as_Vector4Properties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<Color> var, Color endValue, FFEase easeType, float timeToComplete)
        {
            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<Color> myprop = new FFActionProperty<Color>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<Color>(myprop, easeType);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_ColorProperties == null)
                {
                    _sequence[_sequence.Count - 1].as_ColorProperties = new List<FFActionProperty<Color>>();
                }
                _sequence[_sequence.Count - 1].as_ColorProperties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }

        public void Property(FFRef<Color> var, Color endValue, AnimationCurve curve, float timeToComplete = CurveTime)
        {
            if(timeToComplete == CurveTime)
                timeToComplete = curve.TimeToComplete();

            if (_sequence.Count != 0 && var != null)
            {
                // Set Property
                FFActionProperty<Color> myprop = new FFActionProperty<Color>();
                myprop.var = var;
                myprop.start_value = var.Val;
                myprop.end_value = endValue;
                myprop.curr_time = 0.0f;
                myprop.prev_value = var.Val;

                // total_time cannot be zero
                myprop.total_time = Mathf.Max(timeToComplete, 0.01f);

                SetMuGetter<Color>(myprop, curve);

                // Add to front of sequence
                if (_sequence[_sequence.Count - 1].as_ColorProperties == null)
                {
                    _sequence[_sequence.Count - 1].as_ColorProperties = new List<FFActionProperty<Color>>();
                }
                _sequence[_sequence.Count - 1].as_ColorProperties.Add(myprop);
            }
            else
            {
                Debug.Log("Error in ActionSequence Property call");
            }
        }
        
        /// <summary>
        /// The time taken for the curve to complete.
        /// </summary>
        private const float CurveTime = -1.234567890f; // negative values are not valid
        #endregion Properties
    }

    // MuCalculators for Continuous, SmoothStart, SmoothEnd, SmoothStartEnd
    #region mu Calculators
    // tested
    private static float GetMuContinuous(float curr_time, float total_time)
    {
        return curr_time / total_time; // between 0 and 1
    }

    // tested
    private static float GetMuSmoothStart(float curr_time, float total_time)
    {
        float x = curr_time / total_time; // between 0 and 1
        return x * x;
    }

    // tested
    private static float GetMuSmoothEnd(float curr_time, float total_time)
    {
        float x = curr_time / total_time; // between 0 and 1
        return -((x - 1) * (x - 1)) + 1;
    }

    // tested
    private static float GetMuSmoothStartEnd(float curr_time, float total_time)
    {
        float x = curr_time / total_time; // between 0 and 1
        return -(x * x) * (x - 1.5f) * 2; // f(x) = -(x)^2 (x -1.5) * 2
    }
    #endregion mu Calculators

    // MuGetters for Calculators and Animation Curves
    #region MuGetters
    private static void SetMuGetter<T>(FFActionProperty<T> prop, FFEase ease)
    {
        // E_SmoothStart
        if ((FFEase.E_SmoothStart & ease).Equals(FFEase.E_SmoothStart))
        {
            prop.mu_getter = GetMuSmoothStart;
        }
        // E_SmoothEnd
        else if ((FFEase.E_SmoothEnd & ease).Equals(FFEase.E_SmoothEnd))
        {
            prop.mu_getter = GetMuSmoothEnd;
        }
        // E_SmoothStartEnd
        else if ((FFEase.E_SmoothStartEnd & ease).Equals(FFEase.E_SmoothStartEnd))
        {
            prop.mu_getter = GetMuSmoothStartEnd;
        }
        else //default E_Continuous
        {
            prop.mu_getter = GetMuContinuous;
        }
    }

    private static void SetMuGetter<T>(FFActionProperty<T> prop, AnimationCurve curve)
    {
        Debug.Assert(curve.length > 1, "Using animation curves for FFAction.Property(...) Must have atleast 2 points");

        var firstKeyTime = curve.keys[0].time;
        if (firstKeyTime < 0)
        {
            var keys = curve.keys;
            for (int i = 0; i < curve.keys.Length; ++i)
            {
                keys[i].time += (-firstKeyTime);
            }

            curve.keys = keys;
        }

        prop.mu_getter = (curr_time, total_time) => curve.Evaluate((curr_time/total_time) * curve.keys[curve.keys.Length - 1].time);
    }
    #endregion

    #region FFActionTypes
    // A function which takes nothing and returns nothing
    public delegate void FFActionVoidCall();
    // A function which takes an object and returns nothing
    public delegate void FFActionObjectCall(object obj);
    // A class which holds onto the object which is passed to the object call
    public class FFActionObjectCaller
    {
        public FFActionObjectCaller(FFActionObjectCall _call, object _obj)
        {
            call = _call;
            obj = _obj;
        }
        public FFActionObjectCall call;
        public object obj;
    }

    // Get Ease function
    public delegate float GetMu(float curr_time, float total_time);

    public class FFActionProperty<T>
    {
        public FFRef<T> var;
        public T start_value;
        public T end_value;
        public T prev_value;
        public float curr_time;
        public float total_time;
        public GetMu mu_getter;
    }

    public class FFActionSet
    {
        public Queue<FFActionVoidCall>          as_VoidCalls;
        public Queue<FFActionObjectCaller>      as_ObjectCalls;
        public float                            as_DelayTime;
        public List<FFActionProperty<int>>      as_intProperties;
        public List<FFActionProperty<float>>    as_floatProperties;
        public List<FFActionProperty<Vector2>>  as_Vector2Properties;
        public List<FFActionProperty<Vector3>>  as_Vector3Properties;
        public List<FFActionProperty<Vector4>>  as_Vector4Properties;
        public List<FFActionProperty<Color>>    as_ColorProperties;
    }
    #endregion FFActionTypes

    void Awake()
    {
        FFMessage<FFLocalEvents.TimeChangeEvent>.Connect(OnTimeChangeEvent);
    }
    void OnDestroy()
    {
        FFMessage<FFLocalEvents.TimeChangeEvent>.Disconnect(OnTimeChangeEvent);
    }

    private int OnTimeChangeEvent(FFLocalEvents.TimeChangeEvent e)
    {
        //Debug.LogWarning("TimeChangeEvent: " + e.newCurrentTime); // debug

        for (int i = 0; i < ActionSequenceList.Count; ++i)
        {
            ActionSequenceList[i].seqTime = (float)e.newCurrentTime;
            //Debug.LogWarning("SeqTime: " + ActionSequenceList[i].seqTime); //debug
        }
        return 0;
    }

    public bool unlimitedTimeWarp = false;

    void Update()
    {
        var actSeqListCopy = new List<ActionSequence>(ActionSequenceList);
        for (int i = 0; i < actSeqListCopy.Count; ++i)
        {
            const double timeEpsilon = 0.001f;

            float timeoutTime = 60.0f;
            float timeoutTimer = 0.0f;
            float timeRemaining = (float)((double)(FFSystem.time - actSeqListCopy[i].seqTime) + timeEpsilon);
            var actSeq = actSeqListCopy[i];

            while (actSeq.seqTime + timeEpsilon <= FFSystem.time)
            {
                float dt;
                float dtSeq = FFSystem.dt;
                if (actSeq.affectedByTimeScale)
                    dt = Time.deltaTime;
                else
                    dt = Time.unscaledDeltaTime;
                        

                if (!unlimitedTimeWarp && timeoutTimer > timeoutTime)
                {
                    Debug.LogError("Action Sequence time-warped more than 60 seconds, if this is intended behavior be sure to set unlimited timewarp to true");
                    break;
                }

                if (actSeq.seqData != null && actSeq.seqData.Count != 0)
                {
                    bool finishedSet = true;
                    var actSet = actSeq.seqData;

                    // Paused Sequences do do not get updated
                    if (actSeq.Active == false)
                        break;

                    // Calls (must be first)
                    finishedSet = ActionUpdateCalls(actSet[first]);

                    //Delays
                    #region delays
                    actSet[first].as_DelayTime -= dt;
                    if (actSet[first].as_DelayTime <= 0)
                    {
                        actSet[first].as_DelayTime = 0;
                    }
                    else
                    {
                        finishedSet = false;
                    }
                    #endregion delays

                    // Properties
                    #region int
                    if (actSet[first].as_intProperties != null)
                    {
                        int countIncomplete = 0;
                        for (int j = 0; j < actSet[first].as_intProperties.Count; ++j)
                        {
                            if (FFActionUpdaterInt(actSet[first].as_intProperties[j], dt))
                            {
                                ++countIncomplete; // true == incomplete
                            }
                            else
                            {
                                actSet[first].as_intProperties.RemoveAt(j);
                                --j;
                            }
                        }
                        if (countIncomplete > 0)
                            finishedSet = false;
                    }
                    #endregion Int

                    #region float
                    if (actSet[first].as_floatProperties != null)
                    {
                        int countIncomplete = 0;
                        for (int j = 0; j < actSet[first].as_floatProperties.Count; ++j)
                        {
                            if (FFActionUpdaterFloat(actSet[first].as_floatProperties[j], dt))
                            {
                                ++countIncomplete; // true == incomplete
                            }
                            else
                            {
                                actSet[first].as_floatProperties.RemoveAt(j);
                                --j;
                            }
                        }
                        if (countIncomplete > 0)
                            finishedSet = false;
                    }
                    #endregion float

                    #region Vector2
                    if (actSet[first].as_Vector2Properties != null)
                    {
                        int countIncomplete = 0;
                        for (int j = 0; j < actSet[first].as_Vector2Properties.Count; ++j)
                        {
                            if (FFActionUpdaterVector2(actSet[first].as_Vector2Properties[j], dt))
                            {
                                ++countIncomplete; // true == incomplete
                            }
                            else
                            {
                                actSet[first].as_Vector2Properties.RemoveAt(j);
                                --j;
                            }
                        }
                        if (countIncomplete > 0)
                            finishedSet = false;
                    }
                    #endregion Vector2

                    #region Vector3
                    if (actSet[first].as_Vector3Properties != null)
                    {
                        int countIncomplete = 0;
                        for (int j = 0; j < actSet[first].as_Vector3Properties.Count; ++j)
                        {
                            if (FFActionUpdaterVector3(actSet[first].as_Vector3Properties[j], dt))
                            {
                                ++countIncomplete; // true == incomplete
                            }
                            else
                            {
                                actSet[first].as_Vector3Properties.RemoveAt(j);
                                --j;
                            }
                        }
                        if (countIncomplete > 0)
                            finishedSet = false;
                    }
                    #endregion Vector3

                    #region Vector4
                    if (actSet[first].as_Vector4Properties != null)
                    {
                        int countIncomplete = 0;
                        for (int j = 0; j < actSet[first].as_Vector4Properties.Count; ++j)
                        {
                            if (FFActionUpdaterVector4(actSet[first].as_Vector4Properties[j], dt))
                            {
                                ++countIncomplete; // true == incomplete
                            }
                            else
                            {
                                actSet[first].as_Vector4Properties.RemoveAt(j);
                                --j;
                            }
                        }
                        if (countIncomplete > 0)
                            finishedSet = false;
                    }
                    #endregion Vector4

                    #region Color
                    if (actSet[first].as_ColorProperties != null)
                    {
                        int countIncomplete = 0;
                        for (int j = 0; j < actSet[first].as_ColorProperties.Count; ++j)
                        {
                            if (FFActionUpdaterColor(actSet[first].as_ColorProperties[j], dt))
                            {
                                ++countIncomplete; // true == incomplete
                            }
                            else
                            {
                                actSet[first].as_ColorProperties.RemoveAt(j);
                                --j;
                            }
                        }
                        if (countIncomplete > 0)
                            finishedSet = false;
                    }
                    #endregion

                    if (finishedSet && ActionSequenceList[i].seqData.Count > 1)
                    {
                        ActionSequenceList[i].seqData.RemoveAt(first);

                        // Calls of the new set are called this frame since they aren't dependent on time
                        bool finishedCalls;
                        do
                        {
                            finishedCalls = ActionUpdateCalls(ActionSequenceList[i].seqData[first]);
                        } while (finishedCalls == false);

                        InitFFActionSet(ActionSequenceList[i].seqData[first]);

                    }
                }

                timeRemaining -= dt;
                timeoutTimer += dt;
                actSeq.seqTime += dtSeq;
            }
        }
        ActionSequenceList.RemoveAll(item => item == null);
    }

    // FFActionUpdaters (Interpolation logic)
    #region FFActionUpdaters

    // Un-tested
    private static bool FFActionUpdaterInt(FFActionProperty<int> prop, float dt)
    {
        bool incomplete = true;

        // Add dt
        prop.curr_time = dt + prop.curr_time;

        if (prop.curr_time >= prop.total_time)
        {
            prop.curr_time = prop.total_time;
            incomplete = false;
        }

        float mu = prop.mu_getter(prop.curr_time, prop.total_time);
        int next_value = (int)(prop.start_value * (1 - mu) + prop.end_value * mu);

        // added delta if any
        prop.var.Val = next_value - prop.prev_value + prop.var.Val;
        prop.prev_value = next_value;

        return incomplete;
    }
    // Un-tesed
    private static bool FFActionUpdaterFloat(FFActionProperty<float> prop, float dt)
    {
        bool incomplete = true;

        // Add dt
        prop.curr_time = dt + prop.curr_time;

        if (prop.curr_time >= prop.total_time)
        {
            prop.curr_time = prop.total_time;
            incomplete = false;
        }

        float mu = prop.mu_getter(prop.curr_time, prop.total_time);
        float next_value = (prop.start_value * (1 - mu) + prop.end_value * mu);

        // added delta if any
        prop.var.Val = next_value - prop.prev_value + prop.var.Val;
        prop.prev_value = next_value;

        return incomplete;
    }
    // Un-Tested
    private static bool FFActionUpdaterVector2(FFActionProperty<Vector2> prop, float dt)
    {
        bool incomplete = true;

        // Add dt
        prop.curr_time = dt + prop.curr_time;

        if (prop.curr_time >= prop.total_time)
        {
            prop.curr_time = prop.total_time;
            incomplete = false;
        }

        float mu = prop.mu_getter(prop.curr_time, prop.total_time);
        Vector2 next_value = (prop.start_value * (1 - mu) + prop.end_value * mu);

        // added delta
        Vector2 curr_value = prop.var.Val;
        prop.var.Setter(new Vector2(
            next_value.x - prop.prev_value.x + curr_value.x,
            next_value.y - prop.prev_value.y + curr_value.y));

        prop.prev_value.Set(next_value.x, next_value.y);


        return incomplete;
    }
    // tested
    private static bool FFActionUpdaterVector3(FFActionProperty<Vector3> prop, float dt)
    {
        bool incomplete = true;

        // Add dt
        prop.curr_time = dt + prop.curr_time;

        if (prop.curr_time >= prop.total_time)
        {
            prop.curr_time = prop.total_time;
            incomplete = false;
        }

        float mu = prop.mu_getter(prop.curr_time, prop.total_time);
        Vector3 next_value = (prop.start_value * (1 - mu) + prop.end_value * mu);

        // added delta if any
        Vector3 curr_value = prop.var.Val;
        prop.var.Setter(new Vector3(
            next_value.x - prop.prev_value.x + curr_value.x,
            next_value.y - prop.prev_value.y + curr_value.y,
            next_value.z - prop.prev_value.z + curr_value.z));
        prop.prev_value.Set(next_value.x, next_value.y, next_value.z);

        return incomplete;
    }
    // Un-Tested
    private static bool FFActionUpdaterVector4(FFActionProperty<Vector4> prop, float dt)
    {
        bool incomplete = true;

        // Add dt
        prop.curr_time = dt + prop.curr_time;

        if (prop.curr_time >= prop.total_time)
        {
            prop.curr_time = prop.total_time;
            incomplete = false;
        }

        float mu = prop.mu_getter(prop.curr_time, prop.total_time);
        Vector4 next_value = (prop.start_value * (1 - mu) + prop.end_value * mu);

        // added delta if any
        Vector4 curr_value = prop.var.Val;
        prop.var.Setter(new Vector4(
            next_value.x - prop.prev_value.x + curr_value.x,
            next_value.y - prop.prev_value.y + curr_value.y,
            next_value.z - prop.prev_value.z + curr_value.z,
            next_value.w - prop.prev_value.w + curr_value.w));
        prop.prev_value.Set(next_value.x, next_value.y, next_value.z, next_value.w);

        return incomplete;
    }

    private static bool FFActionUpdaterColor(FFActionProperty<Color> prop, float dt)
    {
        bool incomplete = true;

        // Add dt
        prop.curr_time = dt + prop.curr_time;

        if (prop.curr_time >= prop.total_time)
        {
            prop.curr_time = prop.total_time;
            incomplete = false;
        }

        float mu = prop.mu_getter(prop.curr_time, prop.total_time);
        Color next_value = (prop.start_value * (1 - mu) + prop.end_value * mu);

        // added delta if any
        Color curr_value = prop.var.Val;
        prop.var.Setter(new Color(
            next_value.r - prop.prev_value.r + curr_value.r,
            next_value.g - prop.prev_value.g + curr_value.g,
            next_value.b - prop.prev_value.b + curr_value.b,
            next_value.a - prop.prev_value.a + curr_value.a));

        prop.prev_value = new Color(next_value.r, next_value.g, next_value.b, next_value.a);
        return incomplete;
    }
    #endregion FFActionUpdaters

    // calls (must be first thing to update)
    bool ActionUpdateCalls(FFActionSet set)
    {
        bool finishedSet = true;

        // store count so that added calls aren't called
        int voidcallcount;
        if (set.as_VoidCalls != null)
            voidcallcount = set.as_VoidCalls.Count;
        else
            voidcallcount = 0;

        // store count so that added calls aren't called
        int objectcallcount;
        if (set.as_ObjectCalls != null)
            objectcallcount = set.as_ObjectCalls.Count;
        else
            objectcallcount = 0;
        
        // void calls
        for (int j = 0; j < voidcallcount && set.as_VoidCalls != null; ++j)
        {
            set.as_VoidCalls.Dequeue()(); // call dequed function
        }

        // if any calls were added durring these calls, the set is not complete
        if (set.as_VoidCalls != null && set.as_VoidCalls.Count > 0)
            finishedSet = false;


        // object calls
        for (int j = 0; j < objectcallcount && set.as_ObjectCalls != null; ++j)
        {
            var caller = set.as_ObjectCalls.Dequeue(); // deque caller
            caller.call(caller.obj);
        }

        // if any calls were added durring these calls, the set is not complete
        if (set.as_ObjectCalls != null && set.as_ObjectCalls.Count > 0)
            finishedSet = false;

        return finishedSet;
    }

    // Get Fresh values at the start of a new FFActionSet
    void InitFFActionSet(FFActionSet set)
    {
        // InitFFActionSet
        #region int
        if (set.as_intProperties != null)
        {
            for (int i = 0; i < set.as_intProperties.Count; ++i)
            {
                set.as_intProperties[i].start_value = set.as_intProperties[i].var.Val;
            }
        }
        #endregion Int

        #region float
        if (set.as_floatProperties != null)
        {
            for (int i = 0; i < set.as_floatProperties.Count; ++i)
            {
                set.as_floatProperties[i].start_value = set.as_floatProperties[i].var.Val;
            }
        }
        #endregion float

        #region Vector2
        if (set.as_Vector2Properties != null)
        {
            for (int i = 0; i < set.as_Vector2Properties.Count; ++i)
            {
                set.as_Vector2Properties[i].start_value = set.as_Vector2Properties[i].var.Val;
            }
        }
        #endregion Vector2

        #region Vector3
        if (set.as_Vector3Properties != null)
        {
            for (int i = 0; i < set.as_Vector3Properties.Count; ++i)
            {
                set.as_Vector3Properties[i].start_value = set.as_Vector3Properties[i].var.Val;
            }
        }
        #endregion Vector3

        #region Vector4
        if (set.as_Vector4Properties != null)
        {
            for (int i = 0; i < set.as_Vector4Properties.Count; ++i)
            {
                set.as_Vector4Properties[i].start_value = set.as_Vector4Properties[i].var.Val;
            }
        }
        #endregion Vector4

        #region Color
        if (set.as_ColorProperties != null)
        {
            for (int i = 0; i < set.as_ColorProperties.Count; ++i)
            {
                set.as_ColorProperties[i].start_value = set.as_ColorProperties[i].var.Val;
            }
        }
        #endregion Color
    }

    // Clear everything. This will clear all sequences allowing any
    // new operations you add to the sequence to be first.
    public void ClearAllSequences()
    {
        foreach(var seq in ActionSequenceList)
        {
            seq.ClearSequence();
        }
    }
    /// <summary>
    /// Removing the sequence will stop it from being updated by
    /// the action system essentially deleting it from the system.
    /// </summary>
    public void RemoveSequence(ActionSequence seq)
    {
        ActionSequenceList.Remove(seq);
    }
    /// <summary>
    /// ALL sequence will stop from being updated by the action system
    /// essentially deleting/restarting the FFAction component.
    /// </summary>
    public void RemoveAllSequences()
    {
        ActionSequenceList = new List<ActionSequence>();
    }

    private List<ActionSequence> ActionSequenceList = new List<ActionSequence>();

    // returns a clean fresh sequence for use.
    public ActionSequence Sequence()
    {
        FFSystem.GetReady();
        ActionSequence myseq = new ActionSequence(this);
        ActionSequenceList.Add(myseq);
        return myseq;
    }

    // For readability
    private static readonly int first = 0;
}