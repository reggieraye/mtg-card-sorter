# MTG card sorter

This repo is a clearinghouse for everything that's needed to build a two-axis gantry with a suction cup that picks Magic: The Gathering cards off a stack one at a time and drops them into bins. 

The sorting part (camera, card recognition) is not in this repo yet. First the machine has to move one card at a time, reliably. See `SPEC.md` §1 for the exact finish line.

## Files

| Path | What it is |
|---|---|
| `SPEC.md` | What the machine is and does. Read this first. Everything else follows it. |
| `BOM.csv` | Every part, where to get it, rough price, whether we have it. |
| `cad/` | Onshape link + exported STLs of every printed part. |
| `firmware/` | Klipper `printer.cfg`, wiring, and step-by-step setup. |
| `control/` | Empty until a camera is added. |
| `bench/` | Test logs and the plot. |

## Build order

Follow `SPEC.md` §10. Six steps, in order, each with a "done when."

Current step: 1 (ordering parts, starting CAD).

## Who does what

- Joe: CAD, printing, assembly, wiring, running tests, writing up results.
- Mr. Raye: reviews CAD, maintains `firmware/printer.cfg` and `firmware/wiring.md`, helps debug.

## Working rules

- Change `SPEC.md` before changing the machine, not after.
- One commit per working session. Commit message says what changed and what is next.
- Photos of the physical build go in `bench/photos/`, named by date.
