# SPEC — MTG card pick-and-place gantry (MVP)

Owner: Joe Fraser. Advisor: Mr. Raye and Mr. Agro. Last changed: 2026-11-11.

This file is the single source of truth. If something here is wrong, change it here first.

## 1. Goal

Done means: the machine moves 100 cards from the source stack to the bins, one at a time, and at least 95 of those 100 cycles move exactly one card. No jams. At least 6 cards per minute (one card every 10 seconds).

Nothing else counts. Card recognition, Scryfall, and sorting by color are next year's problem.

## 2. What the machine does

One cycle:

1. Move X to the source stack.
2. Move Z down until the touch switch on the suction head trips (the cup is pressing on the top card).
3. Pump on. Wait 300 ms.
4. Move Z up 5 mm slowly (this peels one card off the stack). Then up to travel height.
5. Move X to bin A or bin B. (MVP alternates A, B, A, B. Later, a camera decides.)
6. Pump off. Wait 500 ms. The card drops into the bin.
7. Repeat.

## 3. Layout

Two axes. X moves the head left-right. Z moves the head up-down. The stacks and bins do not move.

```
 top view            X ─────────────────────────────>

  [ bin A ]      [ source stack ]      [ bin B ]
    X = 0           X = 150 mm          X = 300 mm
```

Fixed numbers:

| Item | Value |
|---|---|
| Card | 63 × 88 mm, ~1.8 g, ~0.3 mm thick |
| Source stack capacity | 100 cards (≈ 30 mm tall) |
| X travel | 300 mm |
| Z travel | 60 mm (Z = −2 to 60; the Z endstop is at the top) |
| Z = 0 | the floor of the source tray |
| Travel height (Z while moving in X) | 45 mm |
| Bins | open boxes, 70 × 95 mm inside, 40 mm tall. Cards drop in. |
| Source tray | same as a bin |

## 4. Motion

- X: NEMA 17 stepper, GT2 belt, 20-tooth pulley, idler at the far end. Head rides on two 8 mm hardened steel rods with LM8UU bearings (4 bearings). Endstop switch at X = 0.
- Z: NEMA 17 stepper, T8×8 lead screw (8 mm per turn), brass nut. Head rides on two 8 mm rods with LM8UU bearings (2 bearings). Endstop switch at the top.
- Frame: two 2020 base rails (460 mm), two 2020 posts (178 mm), a laser-cut 6 mm deck on the rails. The X rods run between two printed end blocks bolted to the backs of the posts. All other custom parts are 3D printed.

Why these: every part is a standard 3D-printer part. The designLab has most of them, and every one has a known Klipper config.

## 5. Suction head

- One silicone suction cup, 10–15 mm, on a plunger that slides 3 mm against a light spring. A microswitch sits behind the plunger. When the cup presses on a card, the plunger slides and the switch trips. That is the touch switch in §2 step 2.
- The cup is centered left-right on the card and offset 20 mm from one short edge. Lifting off-center peels the card instead of lifting it flat, which is what sheds a second card.
- Pump: DFRobot 370 mini vacuum pump, 12 V. Tubing: 4 mm ID silicone. The pump plugs into the board's FAN1 port, which Klipper switches on and off. Releasing a card = pump off. If release is too slow (> 1 s), add a small vent solenoid; that is the only reason to add one.

Tunables, in order of what to try when double-feeds happen: peel speed in step 4, dwell in step 3, cup offset from the edge (a CAD change).

No separator lip: the stack top moves down as cards leave, so a fixed lip would only work for the first few cards.

## 6. Electronics

| Part | Role |
|---|---|
| BIGTREETECH Manta M4P | Motor control board. Drives 2 steppers, reads 3 switches, switches the pump. |
| BIGTREETECH CB1 | Small Linux computer that plugs into the Manta. Runs Klipper + Mainsail. |
| 2 × TMC2209 drivers | One per stepper. |
| 12 V 5 A power supply | Powers everything. 12 V so the pump can run straight off a fan port. |
| 3 × microswitches | X endstop, Z endstop, touch switch. |

Wiring: `firmware/wiring.md`.

## 7. Software

Klipper runs on the CB1. Everything is in `firmware/printer.cfg`: motor pins, endstops, the touch switch, the pump, the tunable numbers (`SETTINGS`), and three commands:

- `PICK` — steps 1–4 of §2.
- `PLACE BIN=A` — steps 5–6.
- `SORT_RUN N=100` — runs PICK then PLACE, N times, alternating bins.

Joe runs these from the Mainsail web page in a browser. No Python is needed for the MVP.

## 8. Test

`bench/log.csv`, one row per cycle (columns in `bench/README.md`). `cards_moved` is 0, 1, or 2+ (counted by eye, or by weighing the bins at the end). Run `SORT_RUN N=100` at peel speeds 2, 5, and 10 mm/s (`peel_speed` in `SETTINGS`). The plot of peel speed vs. double-feed rate is the STEM Scholars result.

## 9. Out of scope

Camera, OpenCV, Scryfall, sorting by color, moving stacks, more than two bins, a case, a touchscreen. Each of these is a separate project after §1 is met.

## 10. Build order

Do these in order. Do not start a step until the one before is done.

1. Order the BOM (`BOM.csv`). While waiting: CAD.
2. CAD in Onshape: run `cad/mtg_card_sorter.fs` (see `cad/README.md`). Check fit, change what you need, export STLs to `cad/stl/`. Done when every printed part exists as an STL and nothing collides at any head position.
3. Print and assemble the frame and both axes. Done when the head slides freely by hand along X and Z.
4. Wire, flash, and configure: follow `firmware/README.md` steps 1–5. Done when `G28` homes both axes and `PROBE` on the empty tray reads z ≈ 0.
5. Mount the suction head and pump. Done when `PICK` lifts a card and `PLACE` drops it.
6. Run `SORT_RUN N=100`. Log results. Tune (§5). Done when §1 is met.
