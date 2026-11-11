# Wiring

Power off and unplug before touching any wire. Board pin diagram: page 8 of the [Manta M4P manual](https://github.com/bigtreetech/Manta-M4P/blob/master/BIGTREETECH_MANTA_M4P_User_Manual.pdf).

## Connections

| Part | Board port | How |
|---|---|---|
| 12 V power supply | DCIN | + to 24V terminal, − to GND. (The board label says 24V; 12 V is fine.) |
| X motor | X motor connector (next to X_DRIVER) | Plug in. If X moves the wrong way, fix it in `printer.cfg` (see README), not by rewiring. |
| Z motor | Z motor connector (either one) | Same. |
| X endstop switch | X_STOP | Switch COM → GND pin. Switch NC → signal pin (PC0). Nothing on 5V. |
| Z endstop switch (top of Z) | Z_STOP | Same: COM → GND, NC → PC2. |
| Touch switch (in the suction head) | BLTOUCH header | COM → GND, NC → PC14. |
| Vacuum pump | FAN1 | Red → +, black → −. |

Microswitches have three terminals: COM, NO, NC. Use COM and NC. Then a broken wire reads as "pressed," so the machine stops instead of crashing.

## Jumpers

| Where | Set to | Why |
|---|---|---|
| Under the X_DRIVER and Z_DRIVER sockets | UART mode (manual §3.4) | Lets Klipper set the motor current. |
| Driver power jumper beside each driver | Right side (main POWER) | Motors run off the 12 V supply. |
| FAN POWER | 24V side (= whatever DCIN is, here 12 V) | The pump is a 12 V pump. |

## Drivers

TMC2209s go in X_DRIVER and Z_DRIVER. Match the arrow/EN pin to the socket marking. Y_DRIVER and E0_DRIVER stay empty.
