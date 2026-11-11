# firmware

- `printer.cfg`: the whole control program (Klipper config + commands).
- `wiring.md`: what plugs in where.

Do these in order. Each step has a check; don't go on until it passes.

## 1. Set up the CB1 (the small computer)

1. Download the latest CB1 image from [bigtreetech/CB1 releases](https://github.com/bigtreetech/CB1/releases). It comes with Klipper and Mainsail installed.
2. Write it to a microSD card with [balenaEtcher](https://etcher.balena.io/).
3. On the card, open `system.cfg` and put in the school Wi-Fi name and password. (Or plug in Ethernet.)
4. Put the card in the M4P, power on, wait 2 minutes.
5. In a browser, go to `http://BTT-CB1.local`. Check: the Mainsail page loads.

## 2. Flash the board

Follow the [M4P manual](https://github.com/bigtreetech/Manta-M4P/blob/master/BIGTREETECH_MANTA_M4P_User_Manual.pdf), §6.1–6.3 (log in with user `biqu`, password `biqu`). In `make menuconfig` choose:

- Enable extra low-level configuration options
- STM32 → STM32G0B1
- 8KiB bootloader
- 8 MHz crystal
- USB (on PA11/PA12)

Check: `ls /dev/serial/by-id/` prints one line starting `usb-Klipper_stm32g0b1`.

## 3. Load the config

1. In Mainsail: Machine → upload `printer.cfg` (replace the old one).
2. Paste the line from step 2 into `serial:` at the top.
3. SAVE & RESTART. Check: no red error, and the console says Ready.

## 4. First moves (one hand on the power switch)

Type each line in the Mainsail console.

| Type | Should happen | If not |
|---|---|---|
| `QUERY_ENDSTOPS` | x and z `open` (y says `TRIGGERED`; ignore it, nothing is plugged in there). Press a switch by hand, run again: that one says `TRIGGERED`. | Check wiring.md. |
| `QUERY_PROBE` | Same test for the touch switch. | Same. |
| `G28` | Head goes UP to the Z switch, then LEFT to the X switch, and stops. | Power off. Flip that axis: add or remove `!` in its `dir_pin`. |
| `G1 X150 F3000` | Head moves to the middle, over the source tray. | |
| `SET_PIN PIN=pump VALUE=1` then `VALUE=0` | Pump runs, then stops. | Check the FAN POWER jumper. |

## 5. Calibrate Z = 0

This makes Z = 0 the floor of the source tray. With the tray empty:

1. `G28`, then `G1 X150 F3000`, then `PROBE`.
2. The console says `z=…`. Say it says `z=3.2`.
3. In `printer.cfg`, set `position_endstop` and `position_max` under `[stepper_z]` to 60 − 3.2 = `56.8`. SAVE & RESTART.
4. Repeat 1. Check: it now says z between −0.5 and 0.5.

If `PROBE` says `No trigger`, the floor is too far down: set both numbers to `70`, SAVE & RESTART, and start again at 1.

## 6. Run

1. Put cards in the tray. `G28`.
2. `PICK`, then `PLACE BIN=A`. Check: one card lands in bin A.
3. `SORT_RUN N=10`, then `SORT_RUN N=100`.

Tune by changing numbers in `[gcode_macro SETTINGS]`, then SAVE & RESTART. Log every run in `bench/log.csv`.
