FeatureScript 2641;
import(path : "onshape/std/geometry.fs", version : "2641.0");

// MTG card sorter: builds the whole machine from the numbers in SPEC.md.
//
// How to use: in Onshape, create a Feature Studio, select all, paste this file.
// (If Onshape flags the first two lines, replace them with the two lines a new
// Feature Studio starts with.) Then in a Part Studio: Custom features > MTG card sorter.
//
// Dialog: drag Head X / Head Z to move the head and check that nothing hits.
// Printed parts are colored. Gray parts are bought (rods, motors, extrusion) and are
// only there to check fit. Right-click a colored part > Export to get an STL.
//
// All sizes are mm. World axes: X = Klipper X (head over bin A at 0, source tray at
// 150, bin B at 300). Y points from the front of the machine to the back.
// Z = 0 is the floor of the source tray (Klipper Z = height of the cup tip).

// ---------- Cards, tray, bins (SPEC section 3) ----------
const CARD_W = 63;
const CARD_L = 88;
const STACK_H = 30;            // 100 cards
const BIN_IN_X = 70;
const BIN_IN_Y = 95;
const BIN_WALL = 2;
const BIN_H = 40;              // wall top
const BIN_FLOOR = 2;           // floor thickness (below Z = 0)
const CUP_Y = -CARD_L / 2 + 20; // cup sits 20 mm in from the front short edge

// ---------- Bought hardware ----------
const ROD_D = 8;               // 8 mm linear rod
const BRG_D = 15;              // LM8UU outside diameter
const NEMA = 42.3;             // NEMA 17 body width

// ---------- Layout (change with care: the parts are placed relative to these) ----------
const Y_Z = 64;                // plane of the Z rods and lead screw
const Z_ROD_DX = 20;           // Z rods are this far left/right of the lead screw
const Y_PLATE = 78;            // front face of the X carriage plate
const PLATE_T = 6;
const Y_XROD = 100;            // the two X rods
const XROD_Z1 = 70;
const XROD_Z2 = 150;
const XROD_X0 = -54;           // 400 mm rods
const XROD_X1 = 346;
const Y_BELT = 117;            // belt plane
const BELT_Z = 110;            // pulley center height
const PITCH_R = 6.37;          // GT2 20-tooth pitch radius
const POST_Y0 = 60;            // 2020 posts
const POST_Y1 = 80;
const POST_TOP = 170;
const END_L = [-58, -38];      // x range of the left post / end block
const END_R = [338, 358];
const FLANGE_BOT = [36, 44];   // z range of the X carriage's bottom flange
const FLANGE_TOP = [178, 186]; // z range of the top flange (Z motor sits on it)
const MOTOR_X = -48;           // X motor and idler centers
const IDLER_X = 348;

const RED = color(0.85, 0.35, 0.3);
const BLUE = color(0.2, 0.45, 0.85);
const GREEN = color(0.25, 0.7, 0.4);
const YELLOW = color(0.95, 0.75, 0.2);
const PURPLE = color(0.55, 0.3, 0.75);
const WOOD = color(0.8, 0.7, 0.5);
const GRAY = color(0.6, 0.6, 0.62);

const HEAD_X_BOUNDS = { (millimeter) : [0, 150, 300] } as LengthBoundSpec;
const HEAD_Z_BOUNDS = { (millimeter) : [-2, 45, 60] } as LengthBoundSpec;
const CLEARANCE_BOUNDS = { (millimeter) : [0, 0.2, 1] } as LengthBoundSpec;

annotation { "Feature Type Name" : "MTG card sorter" }
export const mtgCardSorter = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Head X (0 = bin A, 150 = tray, 300 = bin B)" }
        isLength(definition.headX, HEAD_X_BOUNDS);

        annotation { "Name" : "Head Z (cup tip height)" }
        isLength(definition.headZ, HEAD_Z_BOUNDS);

        annotation { "Name" : "Hole clearance (added to holes that take rods and bearings)" }
        isLength(definition.clearance, CLEARANCE_BOUNDS);

        annotation { "Name" : "Show bought parts and cards", "Default" : true }
        definition.showReference is boolean;
    }
    {
        const X = definition.headX / millimeter;
        const Z = definition.headZ / millimeter;
        const c = definition.clearance / millimeter;

        binPart(context, id + "binA", "Bin A", 0);
        binPart(context, id + "tray", "Source tray", 150);
        binPart(context, id + "binB", "Bin B", 300);
        xCarriage(context, id + "xcar", X, c);
        zCarriage(context, id + "zcar", X, Z, c);
        plunger(context, id + "plunger", X, Z);
        endBlock(context, id + "endL", false, c);
        endBlock(context, id + "endR", true, c);
        deck(context, id + "deck");
        if (definition.showReference)
            reference(context, id + "ref", X, Z);
    });

// ---------- Printed parts ----------

function binPart(context is Context, id is Id, name is string, xc is number)
{
    const p = newPart(id);
    const ox = BIN_IN_X / 2 + BIN_WALL;
    const oy = BIN_IN_Y / 2 + BIN_WALL;
    addBox(context, p, vector(xc - ox, -oy, -BIN_FLOOR), vector(xc + ox, oy, BIN_H));
    cutBox(context, p, vector(xc - BIN_IN_X / 2, -BIN_IN_Y / 2, 0), vector(xc + BIN_IN_X / 2, BIN_IN_Y / 2, BIN_H + 1));
    finishPart(context, p, name, RED, true);
}

// Rides the X rods on 4 LM8UU. Holds the Z rods, lead screw and Z motor.
function xCarriage(context is Context, id is Id, X is number, c is number)
{
    const p = newPart(id);
    // plate
    addBox(context, p, vector(X - 36, Y_PLATE, FLANGE_BOT[0]), vector(X + 36, Y_PLATE + PLATE_T, FLANGE_TOP[1]));
    // flanges: bottom holds the rod ends, top holds the rods and the Z motor
    addBox(context, p, vector(X - 32, Y_Z - 12, FLANGE_BOT[0]), vector(X + 32, Y_PLATE, FLANGE_BOT[1]));
    addBox(context, p, vector(X - 32, Y_Z - 24, FLANGE_TOP[0]), vector(X + 32, Y_PLATE, FLANGE_TOP[1]));
    for (var s in [-1, 1])
    {
        for (var fl in [FLANGE_BOT, FLANGE_TOP])
        {
            cutCyl(context, p, vector(X + s * Z_ROD_DX, Y_Z, fl[0] - 1), vector(X + s * Z_ROD_DX, Y_Z, fl[1] + 1), ROD_D + c);
        }
    }
    cutCyl(context, p, vector(X, Y_Z, FLANGE_BOT[0] - 1), vector(X, Y_Z, FLANGE_BOT[1] + 1), 10);    // lead screw
    cutCyl(context, p, vector(X, Y_Z, FLANGE_TOP[0] - 1), vector(X, Y_Z, FLANGE_TOP[1] + 1), 22.5);  // motor pilot
    for (var sx in [-1, 1])
    {
        for (var sy in [-1, 1])
        {
            cutCyl(context, p, vector(X + sx * 15.5, Y_Z + sy * 15.5, FLANGE_TOP[0] - 1),
                    vector(X + sx * 15.5, Y_Z + sy * 15.5, FLANGE_TOP[1] + 1), 3.4);
        }
    }
    // X bearing housings (4x LM8UU, press in from the ends)
    for (var zr in [XROD_Z1, XROD_Z2])
    {
        for (var s in [-1, 1])
        {
            const x0 = s > 0 ? X + 10 : X - 34;
            const x1 = s > 0 ? X + 34 : X - 10;
            addBox(context, p, vector(x0, Y_PLATE + PLATE_T - 1, zr - 10.5), vector(x1, Y_XROD, zr + 10.5));
            addCyl(context, p, vector(x0, Y_XROD, zr), vector(x1, Y_XROD, zr), BRG_D + 6);
            cutCyl(context, p, vector(x0 - 1, Y_XROD, zr), vector(x1 + 1, Y_XROD, zr), BRG_D + c);
        }
    }
    // belt clamp: the top run of the belt goes through the slot; zip-tie it tight
    const zb = BELT_Z + PITCH_R;
    addBox(context, p, vector(X - 10, Y_PLATE + PLATE_T - 1, BELT_Z), vector(X + 10, Y_BELT + 7, BELT_Z + 14));
    cutBox(context, p, vector(X - 11, Y_BELT - 3.5, zb - 1), vector(X + 11, Y_BELT + 3.5, zb + 1));
    for (var s in [-1, 1])
    {
        cutBox(context, p, vector(X + s * 6 - 1.5, Y_BELT - 6, BELT_Z - 1), vector(X + s * 6 + 1.5, Y_BELT + 6, BELT_Z + 15));
    }
    // Z endstop tab, hanging from the top flange
    addBox(context, p, vector(X + 25, Y_Z - 10, 150), vector(X + 28, Y_Z + 10, FLANGE_TOP[0] + 1));
    for (var dy in [-4.75, 4.75])
    {
        cutCyl(context, p, vector(X + 24, Y_Z + dy, 160), vector(X + 29, Y_Z + dy, 160), 2.4);
    }
    finishPart(context, p, "X carriage", BLUE, true);
}

// Rides the Z rods on 2 LM8UU; the lead screw nut lifts it. The arm carries the cup.
function zCarriage(context is Context, id is Id, X is number, Z is number, c is number)
{
    const p = newPart(id);
    addBox(context, p, vector(X - 32, Y_Z - 12, Z + 48), vector(X + 32, Y_Z + 12, Z + 78));
    for (var s in [-1, 1])
    {
        cutCyl(context, p, vector(X + s * Z_ROD_DX, Y_Z, Z + 47), vector(X + s * Z_ROD_DX, Y_Z, Z + 79), BRG_D + c);
    }
    cutCyl(context, p, vector(X, Y_Z, Z + 47), vector(X, Y_Z, Z + 79), 10.5);    // nut body
    for (var sx in [-1, 1])
    {
        for (var sy in [-1, 1])
        {
            cutCyl(context, p, vector(X + sx * 5.66, Y_Z + sy * 5.66, Z + 68), vector(X + sx * 5.66, Y_Z + sy * 5.66, Z + 79), 2.5);
        }
    }
    // arm out to the cup
    addBox(context, p, vector(X - 8, CUP_Y, Z + 48), vector(X + 8, Y_Z - 11, Z + 56));
    // boss: guides the plunger
    addCyl(context, p, vector(X, CUP_Y, Z + 28), vector(X, CUP_Y, Z + 58), 18);
    cutCyl(context, p, vector(X, CUP_Y, Z + 27), vector(X, CUP_Y, Z + 55), 10 + c);    // plunger bore
    cutCyl(context, p, vector(X, CUP_Y, Z + 54), vector(X, CUP_Y, Z + 59), 3.5);       // pin hole
    cutBox(context, p, vector(X + 3, CUP_Y - 1.75, Z + 33.3), vector(X + 10, CUP_Y + 1.75, Z + 40.2)); // screw slot
    // touch-switch tab
    addBox(context, p, vector(X + 5, CUP_Y - 10, Z + 56), vector(X + 7, CUP_Y + 10, Z + 72));
    for (var dy in [-4.75, 4.75])
    {
        cutCyl(context, p, vector(X + 4, CUP_Y + dy, Z + 66), vector(X + 8, CUP_Y + dy, Z + 66), 2.4);
    }
    finishPart(context, p, "Z carriage", GREEN, true);
}

// Slides up 3 mm in the boss when the cup touches a card; its pin presses the touch switch.
// Spring (6 mm OD x 15) sits on top of it inside the boss. An M3 screw in the slot keeps it in.
function plunger(context is Context, id is Id, X is number, Z is number)
{
    const p = newPart(id);
    addCyl(context, p, vector(X, CUP_Y, Z + 12), vector(X, CUP_Y, Z + 41), 10);
    addCyl(context, p, vector(X, CUP_Y, Z + 41), vector(X, CUP_Y, Z + 60), 3);          // pin
    addCyl(context, p, vector(X, CUP_Y, Z + 22), vector(X, CUP_Y + 13, Z + 22), 4.5);   // tube barb
    cutCyl(context, p, vector(X, CUP_Y, Z + 11), vector(X, CUP_Y, Z + 18), 4.2);        // cup socket
    cutCyl(context, p, vector(X, CUP_Y, Z + 17), vector(X, CUP_Y, Z + 23), 2.5);        // air path
    cutCyl(context, p, vector(X, CUP_Y, Z + 22), vector(X, CUP_Y + 14, Z + 22), 2.5);
    cutCyl(context, p, vector(X, CUP_Y, Z + 35), vector(X + 6, CUP_Y, Z + 35), 2.5);    // M3 retaining screw
    finishPart(context, p, "Plunger", YELLOW, true);
}

// Bolts to the back of a 2020 post. Holds the X rod ends and the X motor (left) or idler (right).
function endBlock(context is Context, id is Id, right is boolean, c is number)
{
    const p = newPart(id);
    const x0 = right ? END_R[0] : END_L[0];
    const x1 = right ? END_R[1] : END_L[1];
    const xm = (x0 + x1) / 2;
    addBox(context, p, vector(x0, POST_Y1, 55), vector(x1, 112, POST_TOP));
    for (var z in [80, 140])    // M5 x 40 into T-nuts in the post
    {
        cutCyl(context, p, vector(xm, POST_Y1 - 1, z), vector(xm, 113, z), 5.3);
        cutCyl(context, p, vector(xm, 92, z), vector(xm, 113, z), 9.5);    // counterbore
    }
    for (var zr in [XROD_Z1, XROD_Z2])    // rod sockets, 16 deep
    {
        const a = right ? x0 - 1 : x1 - 16;
        const b = right ? x0 + 16 : x1 + 1;
        cutCyl(context, p, vector(a, Y_XROD, zr), vector(b, Y_XROD, zr), ROD_D + c);
    }
    cutCyl(context, p, vector(xm, Y_XROD, 54), vector(xm, Y_XROD, XROD_Z1), 2.5);           // M3 set screws
    cutCyl(context, p, vector(xm, Y_XROD, XROD_Z2), vector(xm, Y_XROD, POST_TOP + 1), 2.5);
    // back plate + arms (motor or idler mount)
    addBox(context, p, vector(xm - 25, 124, 85), vector(xm + 25, 129, 135));
    addBox(context, p, vector(x0, 111, 85), vector(x1, 124, 95));
    addBox(context, p, vector(x0, 111, 125), vector(x1, 124, 135));
    if (right)
    {
        cutCyl(context, p, vector(xm, 99, BELT_Z), vector(xm, 130, BELT_Z), 5.3);    // M5 idler bolt
    }
    else
    {
        cutBox(context, p, vector(x0 - 1, 103, 98), vector(x1 + 1, 113, 122));       // pulley pocket
        cutCyl(context, p, vector(xm, 123, BELT_Z), vector(xm, 130, BELT_Z), 22.5);  // motor pilot
        for (var sx in [-1, 1])
        {
            for (var sz in [-1, 1])
            {
                cutCyl(context, p, vector(xm + sx * 15.5, 123, BELT_Z + sz * 15.5), vector(xm + sx * 15.5, 130, BELT_Z + sz * 15.5), 3.4);
            }
        }
        // X endstop tab on top
        addBox(context, p, vector(x0, POST_Y1, POST_TOP - 1), vector(x0 + 6, POST_Y1 + 16, POST_TOP + 15));
        for (var dy in [-4.75, 4.75])
        {
            cutCyl(context, p, vector(x0 - 1, POST_Y1 + 8 + dy, POST_TOP + 9), vector(x0 + 7, POST_Y1 + 8 + dy, POST_TOP + 9), 2.4);
        }
    }
    finishPart(context, p, right ? "End block R (idler)" : "End block L (motor)", PURPLE, true);
}

// Laser-cut 6 mm board on top of the base rails. Tray and bins sit on it.
function deck(context is Context, id is Id)
{
    const p = newPart(id);
    addBox(context, p, vector(-80, -60, -8), vector(380, 80, -2));
    for (var e in [END_L, END_R])
    {
        cutBox(context, p, vector(e[0] - 0.5, POST_Y0 - 0.5, -9), vector(e[1] + 0.5, 81, -1));
    }
    for (var h in [[-65, -40], [365, -40], [100, 70], [200, 70]])
    {
        cutCyl(context, p, vector(h[0], h[1], -9), vector(h[0], h[1], -1), 5.3);
    }
    finishPart(context, p, "Deck (laser-cut 6 mm)", WOOD, true);
}

// ---------- Bought parts, for checking fit only ----------

function reference(context is Context, id is Id, X is number, Z is number)
{
    var p = newPart(id + "rails");
    addBox(context, p, vector(-80, -50, -28), vector(380, -30, -8));
    addBox(context, p, vector(-80, POST_Y0, -28), vector(380, POST_Y1, -8));
    finishPart(context, p, "Rails (2020 x 460)", GRAY, false);

    p = newPart(id + "posts");
    for (var e in [END_L, END_R])
    {
        addBox(context, p, vector(e[0], POST_Y0, -8), vector(e[1], POST_Y1, POST_TOP));
    }
    finishPart(context, p, "Posts (2020 x 178)", GRAY, false);

    p = newPart(id + "xrods");
    for (var zr in [XROD_Z1, XROD_Z2])
    {
        addCyl(context, p, vector(XROD_X0, Y_XROD, zr), vector(XROD_X1, Y_XROD, zr), ROD_D);
    }
    finishPart(context, p, "X rods (8 x 400)", GRAY, false);

    p = newPart(id + "zrods");
    for (var s in [-1, 1])
    {
        addCyl(context, p, vector(X + s * Z_ROD_DX, Y_Z, FLANGE_BOT[0]), vector(X + s * Z_ROD_DX, Y_Z, FLANGE_BOT[0] + 150), ROD_D);
    }
    finishPart(context, p, "Z rods (8 x 150)", GRAY, false);

    p = newPart(id + "zmotor");
    addBox(context, p, vector(X - NEMA / 2, Y_Z - NEMA / 2, FLANGE_TOP[1]), vector(X + NEMA / 2, Y_Z + NEMA / 2, FLANGE_TOP[1] + 40));
    addCyl(context, p, vector(X, Y_Z, 153), vector(X, Y_Z, 178), 19);    // coupler
    addCyl(context, p, vector(X, Y_Z, 16), vector(X, Y_Z, 166), 8);      // lead screw
    finishPart(context, p, "Z motor, coupler, lead screw", GRAY, false);

    p = newPart(id + "xmotor");
    addBox(context, p, vector(MOTOR_X - NEMA / 2, 129, BELT_Z - NEMA / 2), vector(MOTOR_X + NEMA / 2, 169, BELT_Z + NEMA / 2));
    addCyl(context, p, vector(MOTOR_X, 105, BELT_Z), vector(MOTOR_X, 129, BELT_Z), 5);    // shaft
    addCyl(context, p, vector(MOTOR_X, 109, BELT_Z), vector(MOTOR_X, 125, BELT_Z), 16);   // pulley
    finishPart(context, p, "X motor + pulley", GRAY, true);

    p = newPart(id + "idler");
    addCyl(context, p, vector(IDLER_X, 113, BELT_Z), vector(IDLER_X, 121.5, BELT_Z), 18);
    finishPart(context, p, "Idler", GRAY, false);

    p = newPart(id + "cup");
    addCyl(context, p, vector(X, CUP_Y, Z), vector(X, CUP_Y, Z + 12), 14);
    finishPart(context, p, "Suction cup", GRAY, false);

    p = newPart(id + "cards");
    addBox(context, p, vector(150 - CARD_W / 2, -CARD_L / 2, 0), vector(150 + CARD_W / 2, CARD_L / 2, STACK_H));
    finishPart(context, p, "Card stack (100 cards)", color(1, 1, 1), false);
}

// ---------- Helpers: each part = union of "add" solids minus "cut" solids ----------

function newPart(id is Id) returns map
{
    return { "id" : id, "n" : new box(0) };
}

function nextId(p is map, kind is string) returns Id
{
    p.n[] = p.n[] + 1;
    return p.id + kind + ("k" ~ p.n[]);
}

function addBox(context is Context, p is map, lo is Vector, hi is Vector)
{
    fCuboid(context, nextId(p, "add"), { "corner1" : lo * millimeter, "corner2" : hi * millimeter });
}

function cutBox(context is Context, p is map, lo is Vector, hi is Vector)
{
    fCuboid(context, nextId(p, "cut"), { "corner1" : lo * millimeter, "corner2" : hi * millimeter });
}

function addCyl(context is Context, p is map, a is Vector, b is Vector, d is number)
{
    fCylinder(context, nextId(p, "add"), { "bottomCenter" : a * millimeter, "topCenter" : b * millimeter, "radius" : d / 2 * millimeter });
}

function cutCyl(context is Context, p is map, a is Vector, b is Vector, d is number)
{
    fCylinder(context, nextId(p, "cut"), { "bottomCenter" : a * millimeter, "topCenter" : b * millimeter, "radius" : d / 2 * millimeter });
}

function finishPart(context is Context, p is map, name is string, col is Color, merge is boolean)
{
    const adds = qCreatedBy(p.id + "add", EntityType.BODY);
    if (merge && size(evaluateQuery(context, adds)) > 1)
    {
        opBoolean(context, p.id + "union", { "tools" : adds, "operationType" : BooleanOperationType.UNION });
    }
    const cuts = qCreatedBy(p.id + "cut", EntityType.BODY);
    if (size(evaluateQuery(context, cuts)) > 0)
    {
        opBoolean(context, p.id + "subtract", { "tools" : cuts, "targets" : qCreatedBy(p.id + "add", EntityType.BODY),
                    "operationType" : BooleanOperationType.SUBTRACTION });
    }
    const bodies = qCreatedBy(p.id, EntityType.BODY);
    setProperty(context, { "entities" : bodies, "propertyType" : PropertyType.NAME, "value" : name });
    setProperty(context, { "entities" : bodies, "propertyType" : PropertyType.APPEARANCE, "value" : col });
}
