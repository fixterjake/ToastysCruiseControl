﻿using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Config.Reader;

namespace ToastysCruiseControl
{
    public class ToastysCruiseControlClient : BaseScript
    {
        private bool _cruiseControl;
        private float _cruiseSpeed;
        private readonly iniconfig _config = new iniconfig("ToastysCruiseControl", "ToastysCruiseControlConfig.ini");
        private readonly int _toggleCruiseControlKey;
        private VehicleClass _vehClass;
        private static Ped LocalPed => Game.PlayerPed;
        private static Vehicle LocalVehicle => LocalPed.CurrentVehicle;

        private enum InputGroups
        {
            CONTROLLER_DPAD_UP = 0,
            CONTROLLER_X = 0,
            W = 27,
            S = 27,
            HANDBRAKE = 27
        }

        private enum Controls
        {
            CONRTOLLER_DPAD_UP = 27,
            CONTROLLER_X = 99,
            W = 71,
            S = 72,
            HANDBRAKE = 76
        }

        private static bool IsKeyJustPressed(InputGroups inputGroups, Controls control) => Game.IsControlJustPressed((int)inputGroups, (Control)control);
        private static bool IsKeyPressed(InputGroups inputGroups, Controls control) => Game.IsControlPressed((int)inputGroups, (Control)control);

        public ToastysCruiseControlClient()
        {
            _toggleCruiseControlKey = _config.GetIntValue("keybinds", "togglecruisecontrol", 168);
            API.DisableControlAction(0, _toggleCruiseControlKey, true);
            Main();
        }

        private async void Main()
        {
            List<VehicleClass> vehClassesWithoutCruiseControl = new List<VehicleClass>
            {
                VehicleClass.Cycles,
                VehicleClass.Motorcycles,
                VehicleClass.Planes,
                VehicleClass.Helicopters,
                VehicleClass.Boats,
                VehicleClass.Trains
            };

            while (true)
            {
                await Delay(0);

                if (LocalVehicle == null) continue;
                _vehClass = LocalVehicle.ClassType;
                _cruiseSpeed = LocalVehicle.Speed;
                
                if (LocalVehicle.IsInWater || !LocalVehicle.IsEngineRunning || LocalVehicle.Driver != LocalPed || LocalPed.IsDead || LocalVehicle.IsInAir || LocalVehicle.HasCollided ||
                    LocalVehicle.SteeringScale >= 0.675f || LocalVehicle.SteeringScale <= -0.675f || IsKeyJustPressed(InputGroups.S, Controls.S) || _cruiseSpeed * 2.23694 + 0.5 < 20 ||
                    _cruiseSpeed * 2.23694 + 0.5 > 150 || vehClassesWithoutCruiseControl.IndexOf(_vehClass) != -1 || HasTireBurst(LocalVehicle, 0) || HasTireBurst(LocalVehicle, 1) || 
                    HasTireBurst(LocalVehicle, 2) || HasTireBurst(LocalVehicle, 3) || HasTireBurst(LocalVehicle, 4) || HasTireBurst(LocalVehicle, 5) || HasTireBurst(LocalVehicle, 45) || 
                    HasTireBurst(LocalVehicle, 47) || IsKeyJustPressed(InputGroups.HANDBRAKE, Controls.HANDBRAKE) || LocalVehicle.CurrentGear == 0)
                {
                    _cruiseControl = false;
                    continue;
                }
                
                if (API.IsDisabledControlJustPressed(0, _toggleCruiseControlKey) ||
                    IsKeyJustPressed(InputGroups.CONTROLLER_DPAD_UP, Controls.CONRTOLLER_DPAD_UP) &&
                    IsKeyJustPressed(InputGroups.CONTROLLER_X, Controls.CONTROLLER_X))
                {
                    if (!_cruiseControl)
                    {
                        Debug.Write($"[TOASTYSCRUISECONTROL] - KEYBIND: { _toggleCruiseControlKey }.");
                        SetSpeed();
                        _cruiseControl = true;
                    }
                    else _cruiseControl = false;
                }
                
                if (_cruiseControl && IsKeyPressed(InputGroups.W, Controls.W))
                {
                    _cruiseControl = false;
                    AcceleratingToNewSpeed();
                }
            }
        }

        private async void SetSpeed()
        {
            while (true)
            {
                await Delay(0);
                
                if (!_cruiseControl) break;
                LocalVehicle.Speed = _cruiseSpeed;
                LocalVehicle.CurrentRPM = 0.5f;
            }
        }

        private async void AcceleratingToNewSpeed()
        {
            while (IsKeyPressed(InputGroups.W, Controls.W))
            {
                await Delay(0);
            }

            _cruiseControl = true;
            SetSpeed();
        }
        
        /// <param name="tire">Index of tires: 0, 1, 2, 3, 4, 5, 45, 47</param>
        private bool HasTireBurst(Vehicle veh, int tire, bool completely = false)
        {
            return Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, veh, tire, completely);
        }
    }
}