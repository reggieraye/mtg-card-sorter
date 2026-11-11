# cad

`mtg_card_sorter.fs` is an Onshape custom feature that builds the whole machine from the numbers in `SPEC.md`.

## Use it

1. In Onshape, make a new document. Add a Feature Studio (the + at bottom left). Select all of its text, paste in `mtg_card_sorter.fs`. If the first two lines get an error, put back the two lines Onshape started with.
2. Go to the Part Studio. Click Custom features (the puzzle-piece icon), add this document, then click MTG card sorter.
3. Drag Head X and Head Z in the dialog and watch the head move. Nothing should touch anything else. Gray parts are bought; colored parts are printed.

## Printed parts

| Part | Color | Print notes |
|---|---|---|
| X carriage | blue | Standing up, bottom flange on the bed, tree supports. |
| Z carriage | green | Upside down (top face on the bed), tree supports. |
| Plunger | yellow | Standing up, cup socket on the bed. |
| End block L (motor), End block R (idler) | purple | Standing on the bottom face, tree supports for the back plate. |
| Bin A, Source tray, Bin B | red | Open side up. No supports. |
| Deck | wood | Not printed: export the top face as DXF and laser-cut 6 mm plywood. |

PETG, 4 walls, 30% infill. Holes for rods and bearings get the dialog's "Hole clearance" (default 0.2 mm). If bearings are loose or rods won't go in, change it and reprint.

Export: right-click each colored part → Export → STL. Save in `stl/` as `partname_v1.stl`. Bump the number each time you reprint.

## Changing the design

The numbers are at the top of the file, with a comment on each. Change a number, the machine rebuilds. If you change the cup offset, travel height, or Z travel, change `SPEC.md` and `firmware/printer.cfg` to match.

Things to measure on the real parts before printing, and fix in the file if different: suction cup barb diameter (plunger socket is 4.2 mm), microswitch hole spacing (tabs are 9.5 mm apart), T8 nut screw circle (16 mm).
