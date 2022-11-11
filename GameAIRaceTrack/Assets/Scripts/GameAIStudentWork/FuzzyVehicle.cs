// compile_check
// Remove the line above if you are submitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameAI;

// All the Fuzz
using Tochas.FuzzyLogic;
using Tochas.FuzzyLogic.MembershipFunctions;
using Tochas.FuzzyLogic.Evaluators;
using Tochas.FuzzyLogic.Mergers;
using Tochas.FuzzyLogic.Defuzzers;
using Tochas.FuzzyLogic.Expressions;

namespace GameAICourse
{

    public class FuzzyVehicle : AIVehicle
    {

        // TODO create some Fuzzy Set enumeration types, and member variables for:
        // Fuzzy Sets (input and output), one or more Fuzzy Value Sets, and Fuzzy
        // Rule Sets for each output.
        // Also, create some methods to instantiate each of the member variables

        // Here are some basic examples to get you started
        enum FzOutputThrottle {Brake, Coast, Accelerate }
        enum FzOutputWheel { TurnLeft, Straight, TurnRight }
        enum FzInputSpeed { Slow, Medium, Fast }
        enum FzVehiclePosition {OnFarLeft, OnLeftCenter, OnCenter, OnRightCenter, OnFarRight}
        enum FzVehicleDirection {TurningLeft, Straight, TurningRight}

        FuzzySet<FzInputSpeed> fzSpeedSet;
        FuzzySet<FzVehiclePosition> fzVehiclePositionSet;
        FuzzySet<FzVehicleDirection> fzVehicleDirectionSet;

        FuzzySet<FzOutputThrottle> fzThrottleSet;
        FuzzyRuleSet<FzOutputThrottle> fzThrottleRuleSet;

        FuzzySet<FzOutputWheel> fzWheelSet;
        FuzzyRuleSet<FzOutputWheel> fzWheelRuleSet;

        FuzzyValueSet fzInputValueSet = new FuzzyValueSet();

        // These are used for debugging (see ApplyFuzzyRules() call
        // in Update()
        FuzzyValueSet mergedThrottle = new FuzzyValueSet();
        FuzzyValueSet mergedWheel = new FuzzyValueSet();

        // ShoulderMembershipFunction(float minX, Coords p0, Coords p1, float maxX)
        // TriangularMembershipFunction(Coords p0, Coords p1, Coords p2)

        private FuzzySet<FzInputSpeed> GetSpeedSet()
        {
            // copy of ammo set from example
            IMembershipFunction SlowFx   = new ShoulderMembershipFunction(0f, new Coords(0f, 1f), new Coords(25f,0f), 100f);
            IMembershipFunction MediumFx = new TriangularMembershipFunction(new Coords(25f, 0f), new Coords(50f, 1f), new Coords(100f, 0f));
            IMembershipFunction FastFx   = new ShoulderMembershipFunction(0f, new Coords(50f, 0f), new Coords(100f, 1f), 100f);

            FuzzySet<FzInputSpeed> set = new FuzzySet<FzInputSpeed>();

            set.Set(FzInputSpeed.Slow, SlowFx);
            set.Set(FzInputSpeed.Medium, MediumFx);
            set.Set(FzInputSpeed.Fast, FastFx);

            return set;
        }

        // throttle [-1, 1]
        private FuzzySet<FzOutputThrottle> GetThrottleSet()
        {
            IMembershipFunction BrakeFx      = new ShoulderMembershipFunction(-1f, new Coords(-1f, 1f), new Coords(-0.25f, 0f), 1f);
            IMembershipFunction CoastFx      = new TriangularMembershipFunction(new Coords(-0.25f, 0f), new Coords(0f, 1f), new Coords(0.25f, 0f));
            IMembershipFunction AccelerateFx = new ShoulderMembershipFunction(-1.0f, new Coords(0.25f, 0f), new Coords(1.0f, 1.0f), 1.0f);

            FuzzySet<FzOutputThrottle> set = new FuzzySet<FzOutputThrottle>();

            set.Set(FzOutputThrottle.Brake, BrakeFx);
            set.Set(FzOutputThrottle.Coast, CoastFx);
            set.Set(FzOutputThrottle.Accelerate, AccelerateFx);

            return set;
        }

        // turn [-1, 1]
        private FuzzySet<FzOutputWheel> GetWheelSet()
        {
            IMembershipFunction TurnLeftFx  = new ShoulderMembershipFunction(-0.8f, new Coords(-0.8f, 1f), new Coords(-0.25f, 0f), 0.8f);
            IMembershipFunction StraightFx  = new TriangularMembershipFunction(new Coords(-0.25f, 0f), new Coords(0f, 1f), new Coords(0.25f, 0f));
            IMembershipFunction TurnRightFx = new ShoulderMembershipFunction(-0.8f, new Coords(0.25f, 0f), new Coords(0.8f, 1.0f), 0.8f);

            FuzzySet<FzOutputWheel> set = new FuzzySet<FzOutputWheel>();

            set.Set(FzOutputWheel.TurnLeft, TurnLeftFx);
            set.Set(FzOutputWheel.Straight, StraightFx);
            set.Set(FzOutputWheel.TurnRight, TurnRightFx);

            return set;
        }

        private FuzzySet<FzVehiclePosition> GetVehiclePositionSet()
        {
            // negative means car is to the right of center, positive means car is to the left of center
            // seems to max out at around -/+ 2.5 before it starts to come off the track

            IMembershipFunction FarRightFx    = new ShoulderMembershipFunction(-2.5f, new Coords(-2.5f, 1f), new Coords(-1.5f, 0f), 2.5f);
            IMembershipFunction RightCenterFx = new TriangularMembershipFunction(new Coords(-1.6f, 0f), new Coords(-1f, 1f), new Coords(0.5f, 0f));
            IMembershipFunction CenterFx      = new TriangularMembershipFunction(new Coords(-1f, 0f), new Coords(0f, 1f), new Coords(1, 0f));
            IMembershipFunction LeftCenterFx  = new TriangularMembershipFunction(new Coords(0.5f, 0f), new Coords(1f, 1f), new Coords(1.6f, 0f));
            IMembershipFunction FarLeftFx     = new ShoulderMembershipFunction(-2.5f, new Coords(1.5f, 0f), new Coords(2.5f, 1f), 2.5f);

            FuzzySet<FzVehiclePosition> set = new FuzzySet<FzVehiclePosition>();

            set.Set(FzVehiclePosition.OnFarLeft,     FarLeftFx);
            set.Set(FzVehiclePosition.OnLeftCenter,  LeftCenterFx);
            set.Set(FzVehiclePosition.OnCenter,      CenterFx);
            set.Set(FzVehiclePosition.OnRightCenter, RightCenterFx);
            set.Set(FzVehiclePosition.OnFarRight,    FarRightFx);

            return set;
        }

        private FuzzySet<FzVehicleDirection> GetVehicleDirectionSet()
        {
            // -/+ 40-ish
            // left is positive, right is negative value
            IMembershipFunction TurningRightFx = new ShoulderMembershipFunction(-40f, new Coords(-40f, 1f), new Coords(-10f, 0f), 40f);
            IMembershipFunction StraightFx     = new TriangularMembershipFunction(new Coords(-10f, 0f), new Coords(0f, 1f), new Coords(10f, 0f));
            IMembershipFunction TurningLeftFx  = new ShoulderMembershipFunction(-40f, new Coords(10f, 0f), new Coords(40f, 1.0f), 40f);

            FuzzySet<FzVehicleDirection> set = new FuzzySet<FzVehicleDirection>();

            set.Set(FzVehicleDirection.TurningLeft, TurningLeftFx);
            set.Set(FzVehicleDirection.Straight, StraightFx);
            set.Set(FzVehicleDirection.TurningRight, TurningRightFx);

            return set;
        }


        private FuzzyRule<FzOutputThrottle>[] GetThrottleRules()
        {

            FuzzyRule<FzOutputThrottle>[] rules =
            {
                // TODO: Add some rules. Here is an example
                // (Note: these aren't necessarily good rules)
                // If(FzInputSpeed.Slow).Then(FzOutputThrottle.Accelerate),
                // If(FzInputSpeed.Medium).Then(FzOutputThrottle.Accelerate),
                // If(FzInputSpeed.Fast).Then(FzOutputThrottle.Coast)

                If(And(FzInputSpeed.Slow, FzVehicleDirection.Straight)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Medium, FzVehicleDirection.Straight)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Fast, FzVehicleDirection.Straight)).Then(FzOutputThrottle.Coast),

                If(And(FzInputSpeed.Slow, FzVehicleDirection.TurningLeft)).Then(FzOutputThrottle.Coast),
                If(And(FzInputSpeed.Medium, FzVehicleDirection.TurningLeft)).Then(FzOutputThrottle.Coast),
                If(And(FzInputSpeed.Fast, FzVehicleDirection.TurningLeft)).Then(FzOutputThrottle.Brake),

                If(And(FzInputSpeed.Slow, FzVehicleDirection.TurningRight)).Then(FzOutputThrottle.Coast),
                If(And(FzInputSpeed.Medium, FzVehicleDirection.TurningRight)).Then(FzOutputThrottle.Coast),
                If(And(FzInputSpeed.Fast, FzVehicleDirection.TurningRight)).Then(FzOutputThrottle.Brake),
            };

            return rules;
        }

        private FuzzyRule<FzOutputWheel>[] GetWheelRules()
        {

            FuzzyRule<FzOutputWheel>[] rules =
            {
                If(FzVehicleDirection.TurningLeft).Then(FzOutputWheel.TurnRight),
                If(FzVehicleDirection.Straight).Then(FzOutputWheel.Straight),
                If(FzVehicleDirection.TurningRight).Then(FzOutputWheel.TurnLeft),

                If(FzVehiclePosition.OnFarLeft).Then(FzOutputWheel.TurnRight),
                If(FzVehiclePosition.OnLeftCenter).Then(FzOutputWheel.Straight),
                If(FzVehiclePosition.OnCenter).Then(FzOutputWheel.Straight),
                If(FzVehiclePosition.OnRightCenter).Then(FzOutputWheel.Straight),
                If(FzVehiclePosition.OnFarRight).Then(FzOutputWheel.TurnLeft),
            };

            return rules;
        }

        private FuzzyRuleSet<FzOutputThrottle> GetThrottleRuleSet(FuzzySet<FzOutputThrottle> throttle)
        {
            var rules = this.GetThrottleRules();
            return new FuzzyRuleSet<FzOutputThrottle>(throttle, rules);
        }

        private FuzzyRuleSet<FzOutputWheel> GetWheelRuleSet(FuzzySet<FzOutputWheel> wheel)
        {
            var rules = this.GetWheelRules();
            return new FuzzyRuleSet<FzOutputWheel>(wheel, rules);
        }

        protected override void Awake()
        {
            base.Awake();

            StudentName = "Tom Nguyen";

            // Only the AI can control. No humans allowed!
            IsPlayer = false;

        }

        protected override void Start()
        {
            base.Start();

            // TODO: You can initialize a bunch of Fuzzy stuff here
            fzSpeedSet = this.GetSpeedSet();
            fzVehiclePositionSet = this.GetVehiclePositionSet();
            fzVehicleDirectionSet = this.GetVehicleDirectionSet();

            fzThrottleSet = this.GetThrottleSet();
            fzThrottleRuleSet = this.GetThrottleRuleSet(fzThrottleSet);

            fzWheelSet = this.GetWheelSet();
            fzWheelRuleSet = this.GetWheelRuleSet(fzWheelSet);

            // draw line to closestPointOnPath
            // Debug.DrawLine(transform.position, transform.forward, Color.red, 2.5f);
        }

        System.Text.StringBuilder strBldr = new System.Text.StringBuilder();

        override protected void Update()
        {

            // TODO Do all your Fuzzy stuff here and then
            // pass your fuzzy rule sets to ApplyFuzzyRules()
            /***
            You can get the direction (Beziér curve tangent as a vector directed forwards) at the point on the middle of the track closest to the vehicle.
            You can then figure out what side of the tangent the vehicle is.
            You can do this by finding the Vector3.SignedAngle() (or use Cross Product) between the tangent vector and a relative vector formed from closest point on road center to vehicle's transform.position.
            Extract the angle sign with Math.Sign() or Boolean logic and apply it to the distance the vehicle is from the road center.
            You now have a signed distance from road center (e.g. lane position), which you can use as a crisp input.
            ***/
            Vector2 currPosition = new Vector2(transform.position.x, transform.position.z);
            Vector2 closestPointPosition = new Vector2(pathTracker.closestPointOnPath.x, pathTracker.closestPointOnPath.z);
            float distanceToClosestPoint = Vector2.Distance(currPosition, closestPointPosition);

            float signedAngle = Vector3.SignedAngle((transform.position - pathTracker.closestPointOnPath), pathTracker.closestPointDirectionOnPath, Vector3.up);
            float sign = Math.Sign(signedAngle); // negative means car is to the right of center, positive means car is to the left of center

            // Debug.Log("distance to closest point: " + distanceToClosestPoint);
            // Debug.Log("sign = " + sign);
            // Debug.Log("signed distance = " + sign*distanceToClosestPoint);

            fzVehiclePositionSet.Evaluate(sign*distanceToClosestPoint, fzInputValueSet);

            // vehicle direction angle from transform.forward compared to closestPointDirectionOnPath?
            float vehicleDirection = Vector3.SignedAngle(transform.forward, pathTracker.closestPointDirectionOnPath, Vector3.up);
            // Debug.Log("vehicle direction: " + vehicleDirection);
            fzVehicleDirectionSet.Evaluate(vehicleDirection, fzInputValueSet);

            // Remove these once you get your fuzzy rules working.
            // You can leave one hardcoded while you work on the other.
            // Both steering and throttle must be implemented with variable
            // control and not fixed/hardcoded!

            // HardCodeSteering(0f);
            // HardCodeThrottle(0.5f);

            // Simple example of fuzzification of vehicle state
            // The Speed is fuzzified and stored in fzInputValueSet
            fzSpeedSet.Evaluate(Speed, fzInputValueSet);

            // ApplyFuzzyRules evaluates your rules and assigns Thottle and Steering accordingly
            // Also, some intermediate values are passed back for debugging purposes
            // Throttle = someValue; //[-1f, 1f] -1 is full brake, 0 is neutral, 1 is full throttle
            // Steering = someValue; // [-1f, 1f] -1 if full left, 0 is neutral, 1 is full right

            ApplyFuzzyRules<FzOutputThrottle, FzOutputWheel>(
                fzThrottleRuleSet,
                fzWheelRuleSet,
                fzInputValueSet,
                // access to intermediate state for debugging
                out var throttleRuleOutput,
                out var wheelRuleOutput,
                ref mergedThrottle,
                ref mergedWheel
                );


            // Use vizText for debugging output
            // You might also use Debug.DrawLine() to draw vectors on Scene view
            if (vizText != null)
            {
                strBldr.Clear();

                // strBldr.AppendLine($"Demo Output");
                // strBldr.AppendLine($"Comment out before submission");

                // You will probably want to selectively enable/disable printing
                // of certain fuzzy states or rules

                AIVehicle.DiagnosticPrintFuzzyValueSet<FzInputSpeed>(fzInputValueSet, strBldr);
                AIVehicle.DiagnosticPrintFuzzyValueSet<FzVehiclePosition>(fzInputValueSet, strBldr);
                AIVehicle.DiagnosticPrintFuzzyValueSet<FzVehicleDirection>(fzInputValueSet, strBldr);

                AIVehicle.DiagnosticPrintRuleSet<FzOutputThrottle>(fzThrottleRuleSet, throttleRuleOutput, strBldr);
                AIVehicle.DiagnosticPrintRuleSet<FzOutputWheel>(fzWheelRuleSet, wheelRuleOutput, strBldr);

                vizText.text = strBldr.ToString();
            }

            // recommend you keep the base Update call at the end, after all your FuzzyVehicle code so that
            // control inputs can be processed properly (e.g. Throttle, Steering)
            base.Update();
        }

    }
}
