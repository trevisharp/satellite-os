# satellite-os
A educational library to learn programming and operational systems.

## The Challenge

Your task is use the SatelliteOS, a linux-line Operational System and program tasks in C# to control a satellite. Your tasks should:

- Keep the satellite height close to 50 and between 30 and 70 every time. 
- Keep the satellite speed close to 40.
- Send the received signal.
- Rotate to maximize the energy.
- Take care with fuel and energy to extend useful life of the satellite.

### Commands

- ls
- cd
- pwd
- rm
- mv
- mkdir
- touch
- cat
- echo
- jobs
- kill
- clear

### Special Commands

- dotnet
- reset
- load
- save
- view

### Sensors

- 0: Tangencial Speed
- 1: Height
- 2: Fuel
- 3: Energy
- 4: Charge
- 5: Antenna

### Actuators

- 0: Tangencial Motor
- 1: Radial Motor
- 2: Rotation Motor
- 3: Signal

### OS Class functions

- WriteLine(object)
- Sleep(int)
- GetSensor(int) -> float
- SetActuator(int, float) [values between -1 and 1]