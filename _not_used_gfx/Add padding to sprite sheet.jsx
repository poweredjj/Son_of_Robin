// A Photoshop script that views the currently open document as a tightly packed sprite sheet, and
// adds padding (in pixels) in all directions to each sprite.
//
// To use, download the script to: C:\Program Files\Adobe\Adobe Photoshop 2021\Presets\Scripts
// Open the target file, then File - Scripts - Add padding to sprite sheet...

/*
<javascriptresource>
    <name>Add padding to sprite sheet...</name>
</javascriptresource>
*/

app.preferences.rulerUnits = Units.PIXELS;


/*
Code for Import https://scriptui.joonas.me — (Triple click to select): 
{"activeId":10,"items":{"item-0":{"id":0,"type":"Dialog","parentId":false,"style":{"enabled":true,"varName":null,"windowType":"Dialog","creationProps":{"su1PanelCoordinates":false,"maximizeButton":false,"minimizeButton":false,"independent":false,"closeButton":true,"borderless":false,"resizeable":false},"text":"Dialog","preferredSize":[0,0],"margins":16,"orientation":"column","spacing":10,"alignChildren":["center","top"]}},"item-1":{"id":1,"type":"StaticText","parentId":3,"style":{"enabled":true,"varName":null,"creationProps":{"truncate":"none","multiline":false,"scrolling":false},"softWrap":false,"text":"Rows","justify":"left","preferredSize":[0,0],"alignment":null,"helpTip":null}},"item-2":{"id":2,"type":"EditText","parentId":3,"style":{"enabled":true,"varName":null,"creationProps":{"noecho":false,"readonly":false,"multiline":false,"scrollable":false,"borderless":false,"enterKeySignalsOnChange":false},"softWrap":false,"text":"1","justify":"left","preferredSize":[0,0],"alignment":null,"helpTip":null}},"item-3":{"id":3,"type":"Group","parentId":0,"style":{"enabled":true,"varName":null,"preferredSize":[0,0],"margins":0,"orientation":"row","spacing":10,"alignChildren":["left","center"],"alignment":null}},"item-4":{"id":4,"type":"Group","parentId":0,"style":{"enabled":true,"varName":null,"preferredSize":[0,0],"margins":0,"orientation":"row","spacing":10,"alignChildren":["left","center"],"alignment":null}},"item-5":{"id":5,"type":"StaticText","parentId":4,"style":{"enabled":true,"varName":null,"creationProps":{"truncate":"none","multiline":false,"scrolling":false},"softWrap":false,"text":"Columns","justify":"left","preferredSize":[0,0],"alignment":null,"helpTip":null}},"item-6":{"id":6,"type":"EditText","parentId":4,"style":{"enabled":true,"varName":null,"creationProps":{"noecho":false,"readonly":false,"multiline":false,"scrollable":false,"borderless":false,"enterKeySignalsOnChange":false},"softWrap":false,"text":"1","justify":"left","preferredSize":[0,0],"alignment":null,"helpTip":null}},"item-7":{"id":7,"type":"Group","parentId":0,"style":{"enabled":true,"varName":null,"preferredSize":[0,0],"margins":0,"orientation":"row","spacing":10,"alignChildren":["left","center"],"alignment":null}},"item-8":{"id":8,"type":"StaticText","parentId":7,"style":{"enabled":true,"varName":null,"creationProps":{"truncate":"none","multiline":false,"scrolling":false},"softWrap":false,"text":"Padding","justify":"left","preferredSize":[0,0],"alignment":null,"helpTip":null}},"item-9":{"id":9,"type":"EditText","parentId":7,"style":{"enabled":true,"varName":null,"creationProps":{"noecho":false,"readonly":false,"multiline":false,"scrollable":false,"borderless":false,"enterKeySignalsOnChange":false},"softWrap":false,"text":"0","justify":"left","preferredSize":[0,0],"alignment":null,"helpTip":null}},"item-10":{"id":10,"type":"Button","parentId":0,"style":{"enabled":true,"varName":null,"text":"Start","justify":"center","preferredSize":[0,0],"alignment":null,"helpTip":null}}},"order":[0,3,1,2,4,5,6,7,8,9,10],"settings":{"importJSON":true,"indentSize":false,"cepExport":false,"includeCSSJS":true,"showDialog":true,"functionWrapper":false,"afterEffectsDockable":false,"itemReferenceList":"None"}}
*/ 

// DIALOG
// ======
var dialog = new Window("dialog"); 
    dialog.text = "Dialog"; 
    dialog.orientation = "column"; 
    dialog.alignChildren = ["center","top"]; 
    dialog.spacing = 10; 
    dialog.margins = 16; 

// GROUP1
// ======
var group1 = dialog.add("group", undefined, {name: "group1"}); 
    group1.orientation = "row"; 
    group1.alignChildren = ["left","center"]; 
    group1.spacing = 10; 
    group1.margins = 0; 

var statictext1 = group1.add("statictext", undefined, undefined, {name: "statictext1"}); 
    statictext1.text = "Rows"; 

var edittext1 = group1.add('edittext {properties: {name: "edittext1"}}'); 
    edittext1.text = "1"; 

// GROUP2
// ======
var group2 = dialog.add("group", undefined, {name: "group2"}); 
    group2.orientation = "row"; 
    group2.alignChildren = ["left","center"]; 
    group2.spacing = 10; 
    group2.margins = 0; 

var statictext2 = group2.add("statictext", undefined, undefined, {name: "statictext2"}); 
    statictext2.text = "Columns"; 

var edittext2 = group2.add('edittext {properties: {name: "edittext2"}}'); 
    edittext2.text = "1"; 

// GROUP3
// ======
var group3 = dialog.add("group", undefined, {name: "group3"}); 
    group3.orientation = "row"; 
    group3.alignChildren = ["left","center"]; 
    group3.spacing = 10; 
    group3.margins = 0; 

var statictext3 = group3.add("statictext", undefined, undefined, {name: "statictext3"}); 
    statictext3.text = "Padding"; 

var edittext3 = group3.add('edittext {properties: {name: "edittext3"}}'); 
    edittext3.text = "0"; 

// DIALOG
// ======
var button1 = dialog.add("button", undefined, undefined, {name: "button1"}); 
    button1.text = "Start"; 

button1.onClick = function () {
    var docRef = app.activeDocument;
    var baseLayer = docRef.activeLayer;

    var rows = Number(edittext1.text);
    var columns = Number(edittext2.text);
    var padding = Number(edittext3.text);

    var spriteWidth = docRef.width / columns;
    var spriteHeight = docRef.height / rows;
    var paddedWidth = spriteWidth + padding * 2;
    var paddedHeight = spriteHeight + padding * 2;

    docRef.resizeCanvas(paddedWidth * columns,
        paddedHeight * rows,
        AnchorPosition.TOPLEFT);

    for (var r = rows - 1; r >= 0; r--)
    {
        for (var c = columns - 1; c >= 0; c--)
        {
            docRef.activeLayer = baseLayer;
            docRef.selection.select([
                [c * spriteWidth, r * spriteHeight],
                [(c + 1) * spriteWidth, r * spriteHeight],
                [(c + 1) * spriteWidth, (r + 1) * spriteHeight],
                [c * spriteWidth, (r + 1) * spriteHeight]
            ]);
            docRef.selection.translate((c * 2 + 1) * padding,
                (r * 2 + 1) * padding);
        }
    }

    // (0, 0) is top left. select method takes a polygon.
    // var shape = [[100, 20], [500, 20], [500, 50], [100, 50]];
    // docRef.selection.select(shape);
    // docRef.selection.cut();
    
    // shape = [[200, 20], [600, 20], [600, 50], [200, 50]];
    // docRef.selection.select(shape);
    // docRef.paste(true);  // paste into selection

    dialog.close();
};

var button2 = dialog.add("button", undefined, undefined, {name: "button2"}); 
    button2.text = "Close"; 
    button2.onClick = function() { dialog.close(); }

dialog.show();



