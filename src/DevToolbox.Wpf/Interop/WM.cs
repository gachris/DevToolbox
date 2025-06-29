namespace DevToolbox.Wpf.Interop;

internal enum WM
{
    /// <summary>
    /// Specified NULL enumeration value.
    /// </summary>
    NULL = 0x0000,

    /// <summary>
    /// Specified CREATE enumeration value.
    /// </summary>
    CREATE = 0x0001,

    /// <summary>
    /// Specified DESTROY enumeration value.
    /// </summary>
    DESTROY = 0x0002,

    /// <summary>
    /// Specified MOVE enumeration value.
    /// </summary>
    MOVE = 0x0003,

    /// <summary>
    /// Specified SIZE enumeration value.
    /// </summary>
    SIZE = 0x0005,

    /// <summary>
    /// Specified ACTIVATE enumeration value.
    /// </summary>
    ACTIVATE = 0x0006,

    /// <summary>
    /// Specified SETFOCUS enumeration value.
    /// </summary>
    SETFOCUS = 0x0007,

    /// <summary>
    /// Specified KILLFOCUS enumeration value.
    /// </summary>
    KILLFOCUS = 0x0008,

    /// <summary>
    /// Specified ENABLE enumeration value.
    /// </summary>
    ENABLE = 0x000A,

    /// <summary>
    /// Specified SETREDRAW enumeration value.
    /// </summary>
    SETREDRAW = 0x000B,

    /// <summary>
    /// Specified SETTEXT enumeration value.
    /// </summary>
    SETTEXT = 0x000C,

    /// <summary>
    /// Specified GETTEXT enumeration value.
    /// </summary>
    GETTEXT = 0x000D,

    /// <summary>
    /// Specified GETTEXTLENGTH enumeration value.
    /// </summary>
    GETTEXTLENGTH = 0x000E,

    /// <summary>
    /// Specified PAINT enumeration value.
    /// </summary>
    PAINT = 0x000F,

    /// <summary>
    /// Specified CLOSE enumeration value.
    /// </summary>
    CLOSE = 0x0010,

    /// <summary>
    /// Specified QUERYENDSESSION enumeration value.
    /// </summary>
    QUERYENDSESSION = 0x0011,

    /// <summary>
    /// Specified QUIT enumeration value.
    /// </summary>
    QUIT = 0x0012,

    /// <summary>
    /// Specified QUERYOPEN enumeration value.
    /// </summary>
    QUERYOPEN = 0x0013,

    /// <summary>
    /// Specified ERASEBKGND enumeration value.
    /// </summary>
    ERASEBKGND = 0x0014,

    /// <summary>
    /// Specified SYSCOLORCHANGE enumeration value.
    /// </summary>
    SYSCOLORCHANGE = 0x0015,

    /// <summary>
    /// Specified ENDSESSION enumeration value.
    /// </summary>
    ENDSESSION = 0x0016,

    /// <summary>
    /// Specified SHOWWINDOW enumeration value.
    /// </summary>
    SHOWWINDOW = 0x0018,

    /// <summary>
    /// Specified WININICHANGE enumeration value.
    /// </summary>
    WININICHANGE = 0x001A,

    /// <summary>
    /// Specified SETTINGCHANGE enumeration value.
    /// </summary>
    SETTINGCHANGE = 0x001A,

    /// <summary>
    /// Specified DEVMODECHANGE enumeration value.
    /// </summary>
    DEVMODECHANGE = 0x001B,

    /// <summary>
    /// Specified ACTIVATEAPP enumeration value.
    /// </summary>
    ACTIVATEAPP = 0x001C,

    /// <summary>
    /// Specified FONTCHANGE enumeration value.
    /// </summary>
    FONTCHANGE = 0x001D,

    /// <summary>
    /// Specified TIMECHANGE enumeration value.
    /// </summary>
    TIMECHANGE = 0x001E,

    /// <summary>
    /// Specified CANCELMODE enumeration value.
    /// </summary>
    CANCELMODE = 0x001F,

    /// <summary>
    /// Specified SETCURSOR enumeration value.
    /// </summary>
    SETCURSOR = 0x0020,

    /// <summary>
    /// Specified MOUSEACTIVATE enumeration value.
    /// </summary>
    MOUSEACTIVATE = 0x0021,

    /// <summary>
    /// Specified CHILDACTIVATE enumeration value.
    /// </summary>
    CHILDACTIVATE = 0x0022,

    /// <summary>
    /// Specified QUEUESYNC enumeration value.
    /// </summary>
    QUEUESYNC = 0x0023,

    /// <summary>
    /// Specified GETMINMAXINFO enumeration value.
    /// </summary>
    GETMINMAXINFO = 0x0024,

    /// <summary>
    /// Specified PAINTICON enumeration value.
    /// </summary>
    PAINTICON = 0x0026,

    /// <summary>
    /// Specified ICONERASEBKGND enumeration value.
    /// </summary>
    ICONERASEBKGND = 0x0027,

    /// <summary>
    /// Specified NEXTDLGCTL enumeration value.
    /// </summary>
    NEXTDLGCTL = 0x0028,

    /// <summary>
    /// Specified SPOOLERSTATUS enumeration value.
    /// </summary>
    SPOOLERSTATUS = 0x002A,

    /// <summary>
    /// Specified DRAWITEM enumeration value.
    /// </summary>
    DRAWITEM = 0x002B,

    /// <summary>
    /// Specified MEASUREITEM enumeration value.
    /// </summary>
    MEASUREITEM = 0x002C,

    /// <summary>
    /// Specified DELETEITEM enumeration value.
    /// </summary>
    DELETEITEM = 0x002D,

    /// <summary>
    /// Specified VKEYTOITEM enumeration value.
    /// </summary>
    VKEYTOITEM = 0x002E,

    /// <summary>
    /// Specified CHARTOITEM enumeration value.
    /// </summary>
    CHARTOITEM = 0x002F,

    /// <summary>
    /// Specified SETFONT enumeration value.
    /// </summary>
    SETFONT = 0x0030,

    /// <summary>
    /// Specified GETFONT enumeration value.
    /// </summary>
    GETFONT = 0x0031,

    /// <summary>
    /// Specified SETHOTKEY enumeration value.
    /// </summary>
    SETHOTKEY = 0x0032,

    /// <summary>
    /// Specified GETHOTKEY enumeration value.
    /// </summary>
    GETHOTKEY = 0x0033,

    /// <summary>
    /// Specified QUERYDRAGICON enumeration value.
    /// </summary>
    QUERYDRAGICON = 0x0037,

    /// <summary>
    /// Specified COMPAREITEM enumeration value.
    /// </summary>
    COMPAREITEM = 0x0039,

    /// <summary>
    /// Specified GETOBJECT enumeration value.
    /// </summary>
    GETOBJECT = 0x003D,

    /// <summary>
    /// Specified COMPACTING enumeration value.
    /// </summary>
    COMPACTING = 0x0041,

    /// <summary>
    /// Specified COMMNOTIFY enumeration value.
    /// </summary>
    COMMNOTIFY = 0x0044,

    /// <summary>
    /// Specified WINDOWPOSCHANGING enumeration value.
    /// </summary>
    WINDOWPOSCHANGING = 0x0046,

    /// <summary>
    /// Specified WINDOWPOSCHANGED enumeration value.
    /// </summary>
    WINDOWPOSCHANGED = 0x0047,

    /// <summary>
    /// Specified POWER enumeration value.
    /// </summary>
    POWER = 0x0048,

    /// <summary>
    /// Specified COPYDATA enumeration value.
    /// </summary>
    COPYDATA = 0x004A,

    /// <summary>
    /// Specified CANCELJOURNAL enumeration value.
    /// </summary>
    CANCELJOURNAL = 0x004B,

    /// <summary>
    /// Specified NOTIFY enumeration value.
    /// </summary>
    NOTIFY = 0x004E,

    /// <summary>
    /// Specified INPUTLANGCHANGEREQUEST enumeration value.
    /// </summary>
    INPUTLANGCHANGEREQUEST = 0x0050,

    /// <summary>
    /// Specified INPUTLANGCHANGE enumeration value.
    /// </summary>
    INPUTLANGCHANGE = 0x0051,

    /// <summary>
    /// Specified TCARD enumeration value.
    /// </summary>
    TCARD = 0x0052,

    /// <summary>
    /// Specified HELP enumeration value.
    /// </summary>
    HELP = 0x0053,

    /// <summary>
    /// Specified USERCHANGED enumeration value.
    /// </summary>
    USERCHANGED = 0x0054,

    /// <summary>
    /// Specified NOTIFYFORMAT enumeration value.
    /// </summary>
    NOTIFYFORMAT = 0x0055,

    /// <summary>
    /// Specified CONTEXTMENU enumeration value.
    /// </summary>
    CONTEXTMENU = 0x007B,

    /// <summary>
    /// Specified STYLECHANGING enumeration value.
    /// </summary>
    STYLECHANGING = 0x007C,

    /// <summary>
    /// Specified STYLECHANGED enumeration value.
    /// </summary>
    STYLECHANGED = 0x007D,

    /// <summary>
    /// Specified DISPLAYCHANGE enumeration value.
    /// </summary>
    DISPLAYCHANGE = 0x007E,

    /// <summary>
    /// Specified GETICON enumeration value.
    /// </summary>
    GETICON = 0x007F,

    /// <summary>
    /// Specified SETICON enumeration value.
    /// </summary>
    SETICON = 0x0080,

    /// <summary>
    /// Specified NCCREATE enumeration value.
    /// </summary>
    NCCREATE = 0x0081,

    /// <summary>
    /// Specified VK_RMENU enumeration value.
    /// </summary>
    NCDESTROY = 0x0082,

    /// <summary>
    /// Specified NCCALCSIZE enumeration value.
    /// </summary>
    NCCALCSIZE = 0x0083,

    /// <summary>
    /// Specified NCHITTEST enumeration value.
    /// </summary>
    NCHITTEST = 0x0084,

    /// <summary>
    /// Specified NCPAINT enumeration value.
    /// </summary>
    NCPAINT = 0x0085,

    /// <summary>
    /// Specified NCACTIVATE enumeration value.
    /// </summary>
    NCACTIVATE = 0x0086,

    /// <summary>
    /// Specified GETDLGCODE enumeration value.
    /// </summary>
    GETDLGCODE = 0x0087,

    /// <summary>
    /// Specified SYNCPAINT enumeration value.
    /// </summary>
    SYNCPAINT = 0x0088,

    /// <summary>
    /// Specified NCMOUSEMOVE enumeration value.
    /// </summary>
    NCMOUSEMOVE = 0x00A0,

    /// <summary>
    /// Specified NCLBUTTONDOWN enumeration value.
    /// </summary>
    NCLBUTTONDOWN = 0x00A1,

    /// <summary>
    /// Specified NCLBUTTONUP enumeration value.
    /// </summary>
    NCLBUTTONUP = 0x00A2,

    /// <summary>
    /// Specified NCLBUTTONDBLCLK enumeration value.
    /// </summary>
    NCLBUTTONDBLCLK = 0x00A3,

    /// <summary>
    /// Specified NCRBUTTONDOWN enumeration value.
    /// </summary>
    NCRBUTTONDOWN = 0x00A4,

    /// <summary>
    /// Specified NCRBUTTONUP enumeration value.
    /// </summary>
    NCRBUTTONUP = 0x00A5,

    /// <summary>
    /// Specified NCRBUTTONDBLCLK enumeration value.
    /// </summary>
    NCRBUTTONDBLCLK = 0x00A6,

    /// <summary>
    /// Specified NCMBUTTONDOWN enumeration value.
    /// </summary>
    NCMBUTTONDOWN = 0x00A7,

    /// <summary>
    /// Specified NCMBUTTONUP enumeration value.
    /// </summary>
    NCMBUTTONUP = 0x00A8,

    /// <summary>
    /// Specified NCMBUTTONDBLCLK enumeration value.
    /// </summary>
    NCMBUTTONDBLCLK = 0x00A9,

    /// <summary>
    /// Specified NCXBUTTONDOWN enumeration value.
    /// </summary>
    NCXBUTTONDOWN = 0x00AB,

    /// <summary>
    /// Specified NCXBUTTONUP enumeration value.
    /// </summary>
    NCXBUTTONUP = 0x00AC,

    /// <summary>
    /// The MOUSEHWHEEL message Sent to the active window when the mouse's horizontal scroll
    /// wheel is tilted or rotated.
    /// </summary>
    MOUSEHWHEEL = 0x20E,

    /// <summary>
    /// Specified KEYDOWN enumeration value.
    /// </summary>
    KEYDOWN = 0x0100,

    /// <summary>
    /// Specified KEYUP enumeration value.
    /// </summary>
    KEYUP = 0x0101,

    /// <summary>
    /// Specified CHAR enumeration value.
    /// </summary>
    CHAR = 0x0102,

    /// <summary>
    /// Specified DEADCHAR enumeration value.
    /// </summary>
    DEADCHAR = 0x0103,

    /// <summary>
    /// Specified SYSKEYDOWN enumeration value.
    /// </summary>
    SYSKEYDOWN = 0x0104,

    /// <summary>
    /// Specified SYSKEYUP enumeration value.
    /// </summary>
    SYSKEYUP = 0x0105,

    /// <summary>
    /// Specified SYSCHAR enumeration value.
    /// </summary>
    SYSCHAR = 0x0106,

    /// <summary>
    /// Specified SYSDEADCHAR enumeration value.
    /// </summary>
    SYSDEADCHAR = 0x0107,

    /// <summary>
    /// Specified KEYLAST enumeration value.
    /// </summary>
    KEYLAST = 0x0108,

    /// <summary>
    /// Specified IME_STARTCOMPOSITION enumeration value.
    /// </summary>
    IME_STARTCOMPOSITION = 0x010D,

    /// <summary>
    /// Specified IME_ENDCOMPOSITION enumeration value.
    /// </summary>
    IME_ENDCOMPOSITION = 0x010E,

    /// <summary>
    /// Specified IME_COMPOSITION enumeration value.
    /// </summary>
    IME_COMPOSITION = 0x010F,

    /// <summary>
    /// Specified IME_KEYLAST enumeration value.
    /// </summary>
    IME_KEYLAST = 0x010F,

    /// <summary>
    /// Specified INITDIALOG enumeration value.
    /// </summary>
    INITDIALOG = 0x0110,

    /// <summary>
    /// Specified COMMAND enumeration value.
    /// </summary>
    COMMAND = 0x0111,

    /// <summary>
    /// Specified SYSCOMMAND enumeration value.
    /// </summary>
    SYSCOMMAND = 0x0112,

    /// <summary>
    /// Specified TIMER enumeration value.
    /// </summary>
    TIMER = 0x0113,

    /// <summary>
    /// Specified HSCROLL enumeration value.
    /// </summary>
    HSCROLL = 0x0114,

    /// <summary>
    /// Specified VSCROLL enumeration value.
    /// </summary>
    VSCROLL = 0x0115,

    /// <summary>
    /// Specified INITMENU enumeration value.
    /// </summary>
    INITMENU = 0x0116,

    /// <summary>
    /// Specified INITMENUPOPUP enumeration value.
    /// </summary>
    INITMENUPOPUP = 0x0117,

    /// <summary>
    /// Specified MENUSELECT enumeration value.
    /// </summary>
    MENUSELECT = 0x011F,

    /// <summary>
    /// Specified MENUCHAR enumeration value.
    /// </summary>
    MENUCHAR = 0x0120,

    /// <summary>
    /// Specified ENTERIDLE enumeration value.
    /// </summary>
    ENTERIDLE = 0x0121,

    /// <summary>
    /// Specified MENURBUTTONUP enumeration value.
    /// </summary>
    MENURBUTTONUP = 0x0122,

    /// <summary>
    /// Specified MENUDRAG enumeration value.
    /// </summary>
    MENUDRAG = 0x0123,

    /// <summary>
    /// Specified MENUGETOBJECT enumeration value.
    /// </summary>
    MENUGETOBJECT = 0x0124,

    /// <summary>
    /// Specified UNINITMENUPOPUP enumeration value.
    /// </summary>
    UNINITMENUPOPUP = 0x0125,

    /// <summary>
    /// Specified MENUCOMMAND enumeration value.
    /// </summary>
    MENUCOMMAND = 0x0126,

    /// <summary>
    /// Specified CTLCOLORMSGBOX enumeration value.
    /// </summary>
    CTLCOLORMSGBOX = 0x0132,

    /// <summary>
    /// Specified CTLCOLOREDIT enumeration value.
    /// </summary>
    CTLCOLOREDIT = 0x0133,

    /// <summary>
    /// Specified CTLCOLORLISTBOX enumeration value.
    /// </summary>
    CTLCOLORLISTBOX = 0x0134,

    /// <summary>
    /// Specified CTLCOLORBTN enumeration value.
    /// </summary>
    CTLCOLORBTN = 0x0135,

    /// <summary>
    /// Specified CTLCOLORDLG enumeration value.
    /// </summary>
    CTLCOLORDLG = 0x0136,

    /// <summary>
    /// Specified CTLCOLORSCROLLBAR enumeration value.
    /// </summary>
    CTLCOLORSCROLLBAR = 0x0137,

    /// <summary>
    /// Specified CTLCOLORSTATIC enumeration value.
    /// </summary>
    CTLCOLORSTATIC = 0x0138,

    /// <summary>
    /// Specified MOUSEMOVE enumeration value.
    /// </summary>
    MOUSEMOVE = 0x0200,

    /// <summary>
    /// Specified LBUTTONDOWN enumeration value.
    /// </summary>
    LBUTTONDOWN = 0x0201,

    /// <summary>
    /// Specified LBUTTONUP enumeration value.
    /// </summary>
    LBUTTONUP = 0x0202,

    /// <summary>
    /// Specified LBUTTONDBLCLK enumeration value.
    /// </summary>
    LBUTTONDBLCLK = 0x0203,

    /// <summary>
    /// Specified RBUTTONDOWN enumeration value.
    /// </summary>
    RBUTTONDOWN = 0x0204,

    /// <summary>
    /// Specified RBUTTONUP enumeration value.
    /// </summary>
    RBUTTONUP = 0x0205,

    /// <summary>
    /// Specified RBUTTONDBLCLK enumeration value.
    /// </summary>
    RBUTTONDBLCLK = 0x0206,

    /// <summary>
    /// Specified MBUTTONDOWN enumeration value.
    /// </summary>
    MBUTTONDOWN = 0x0207,

    /// <summary>
    /// Specified MBUTTONUP enumeration value.
    /// </summary>
    MBUTTONUP = 0x0208,

    /// <summary>
    /// Specified MBUTTONDBLCLK enumeration value.
    /// </summary>
    MBUTTONDBLCLK = 0x0209,

    /// <summary>
    /// Specified MOUSEWHEEL enumeration value.
    /// </summary>
    MOUSEWHEEL = 0x020A,

    /// <summary>
    /// Specified XBUTTONDOWN enumeration value.
    /// </summary>
    XBUTTONDOWN = 0x020B,

    /// <summary>
    /// Specified XBUTTONUP enumeration value.
    /// </summary>
    XBUTTONUP = 0x020C,

    /// <summary>
    /// Specified XBUTTONDBLCLK enumeration value.
    /// </summary>
    XBUTTONDBLCLK = 0x020D,

    /// <summary>
    /// Specified PARENTNOTIFY enumeration value.
    /// </summary>
    PARENTNOTIFY = 0x0210,

    /// <summary>
    /// Specified ENTERMENULOOP enumeration value.
    /// </summary>
    ENTERMENULOOP = 0x0211,

    /// <summary>
    /// Specified EXITMENULOOP enumeration value.
    /// </summary>
    EXITMENULOOP = 0x0212,

    /// <summary>
    /// Specified NEXTMENU enumeration value.
    /// </summary>
    NEXTMENU = 0x0213,

    /// <summary>
    /// Specified SIZING enumeration value.
    /// </summary>
    SIZING = 0x0214,

    /// <summary>
    /// Specified CAPTURECHANGED enumeration value.
    /// </summary>
    CAPTURECHANGED = 0x0215,

    /// <summary>
    /// Specified MOVING enumeration value.
    /// </summary>
    MOVING = 0x0216,

    /// <summary>
    /// Specified DEVICECHANGE enumeration value.
    /// </summary>
    DEVICECHANGE = 0x0219,

    /// <summary>
    /// Specified MDICREATE enumeration value.
    /// </summary>
    MDICREATE = 0x0220,

    /// <summary>
    /// Specified MDIDESTROY enumeration value.
    /// </summary>
    MDIDESTROY = 0x0221,

    /// <summary>
    /// Specified MDIACTIVATE enumeration value.
    /// </summary>
    MDIACTIVATE = 0x0222,

    /// <summary>
    /// Specified MDIRESTORE enumeration value.
    /// </summary>
    MDIRESTORE = 0x0223,

    /// <summary>
    /// Specified MDINEXT enumeration value.
    /// </summary>
    MDINEXT = 0x0224,

    /// <summary>
    /// Specified MDIMAXIMIZE enumeration value.
    /// </summary>
    MDIMAXIMIZE = 0x0225,

    /// <summary>
    /// Specified MDITILE enumeration value.
    /// </summary>
    MDITILE = 0x0226,

    /// <summary>
    /// Specified MDICASCADE enumeration value.
    /// </summary>
    MDICASCADE = 0x0227,

    /// <summary>
    /// Specified MDIICONARRANGE enumeration value.
    /// </summary>
    MDIICONARRANGE = 0x0228,

    /// <summary>
    /// Specified MDIGETACTIVE enumeration value.
    /// </summary>
    MDIGETACTIVE = 0x0229,

    /// <summary>
    /// Specified MDISETMENU enumeration value.
    /// </summary>
    MDISETMENU = 0x0230,

    /// <summary>
    /// Specified ENTERSIZEMOVE enumeration value.
    /// </summary>
    ENTERSIZEMOVE = 0x0231,

    /// <summary>
    /// Specified EXITSIZEMOVE enumeration value.
    /// </summary>
    EXITSIZEMOVE = 0x0232,

    /// <summary>
    /// Specified DROPFILES enumeration value.
    /// </summary>
    DROPFILES = 0x0233,

    /// <summary>
    /// Specified MDIREFRESHMENU enumeration value.
    /// </summary>
    MDIREFRESHMENU = 0x0234,

    /// <summary>
    /// Specified IME_SETCONTEXT enumeration value.
    /// </summary>
    IME_SETCONTEXT = 0x0281,

    /// <summary>
    /// Specified IME_NOTIFY enumeration value.
    /// </summary>
    IME_NOTIFY = 0x0282,

    /// <summary>
    /// Specified IME_CONTROL enumeration value.
    /// </summary>
    IME_CONTROL = 0x0283,

    /// <summary>
    /// Specified IME_COMPOSITIONFULL enumeration value.
    /// </summary>
    IME_COMPOSITIONFULL = 0x0284,

    /// <summary>
    /// Specified IME_SELECT enumeration value.
    /// </summary>
    IME_SELECT = 0x0285,

    /// <summary>
    /// Specified IME_CHAR enumeration value.
    /// </summary>
    IME_CHAR = 0x0286,

    /// <summary>
    /// Specified IME_REQUEST enumeration value.
    /// </summary>
    IME_REQUEST = 0x0288,

    /// <summary>
    /// Specified IME_KEYDOWN enumeration value.
    /// </summary>
    IME_KEYDOWN = 0x0290,

    /// <summary>
    /// Specified IME_KEYUP enumeration value.
    /// </summary>
    IME_KEYUP = 0x0291,

    /// <summary>
    /// Specified NCMOUSEHOVER enumeration value.
    /// </summary>
    NCMOUSEHOVER = 0x02A0,

    /// <summary>
    /// Specified MOUSEHOVER enumeration value.
    /// </summary>
    MOUSEHOVER = 0x02A1,

    /// <summary>
    /// Specified NCMOUSELEAVE enumeration value.
    /// </summary>
    NCMOUSELEAVE = 0x02A2,

    /// <summary>
    /// Specified MOUSELEAVE enumeration value.
    /// </summary>
    MOUSELEAVE = 0x02A3,

    /// <summary>
    /// Specified CUT enumeration value.
    /// </summary>
    CUT = 0x0300,

    /// <summary>
    /// Specified COPY enumeration value.
    /// </summary>
    COPY = 0x0301,

    /// <summary>
    /// Specified PASTE enumeration value.
    /// </summary>
    PASTE = 0x0302,

    /// <summary>
    /// Specified CLEAR enumeration value.
    /// </summary>
    CLEAR = 0x0303,

    /// <summary>
    /// Specified UNDO enumeration value.
    /// </summary>
    UNDO = 0x0304,

    /// <summary>
    /// Specified RENDERFORMAT enumeration value.
    /// </summary>
    RENDERFORMAT = 0x0305,

    /// <summary>
    /// Specified RENDERALLFORMATS enumeration value.
    /// </summary>
    RENDERALLFORMATS = 0x0306,

    /// <summary>
    /// Specified DESTROYCLIPBOARD enumeration value.
    /// </summary>
    DESTROYCLIPBOARD = 0x0307,

    /// <summary>
    /// Specified DRAWCLIPBOARD enumeration value.
    /// </summary>
    DRAWCLIPBOARD = 0x0308,

    /// <summary>
    /// Specified PAINTCLIPBOARD enumeration value.
    /// </summary>
    PAINTCLIPBOARD = 0x0309,

    /// <summary>
    /// Specified VSCROLLCLIPBOARD enumeration value.
    /// </summary>
    VSCROLLCLIPBOARD = 0x030A,

    /// <summary>
    /// Specified SIZECLIPBOARD enumeration value.
    /// </summary>
    SIZECLIPBOARD = 0x030B,

    /// <summary>
    /// Specified ASKCBFORMATNAME enumeration value.
    /// </summary>
    ASKCBFORMATNAME = 0x030C,

    /// <summary>
    /// Specified CHANGECBCHAIN enumeration value.
    /// </summary>
    CHANGECBCHAIN = 0x030D,

    /// <summary>
    /// Specified HSCROLLCLIPBOARD enumeration value.
    /// </summary>
    HSCROLLCLIPBOARD = 0x030E,

    /// <summary>
    /// Specified QUERYNEWPALETTE enumeration value.
    /// </summary>
    QUERYNEWPALETTE = 0x030F,

    /// <summary>
    /// Specified PALETTEISCHANGING enumeration value.
    /// </summary>
    PALETTEISCHANGING = 0x0310,

    /// <summary>
    /// Specified PALETTECHANGED enumeration value.
    /// </summary>
    PALETTECHANGED = 0x0311,

    /// <summary>
    /// Specified HOTKEY enumeration value.
    /// </summary>
    HOTKEY = 0x0312,

    /// <summary>
    /// Specified PRINT enumeration value.
    /// </summary>
    PRINT = 0x0317,

    /// <summary>
    /// Specified PRINTCLIENT enumeration value.
    /// </summary>
    PRINTCLIENT = 0x0318,

    /// <summary>
    /// Specified HANDHELDFIRST enumeration value.
    /// </summary>
    HANDHELDFIRST = 0x0358,

    /// <summary>
    /// Specified HANDHELDLAST enumeration value.
    /// </summary>
    HANDHELDLAST = 0x035F,

    /// <summary>
    /// Specified AFXFIRST enumeration value.
    /// </summary>
    AFXFIRST = 0x0360,

    /// <summary>
    /// Specified AFXLAST enumeration value.
    /// </summary>
    AFXLAST = 0x037F,

    /// <summary>
    /// Specified PENWINFIRST enumeration value.
    /// </summary>
    PENWINFIRST = 0x0380,

    /// <summary>
    /// Specified PENWINLAST enumeration value.
    /// </summary>
    PENWINLAST = 0x038F,

    /// <summary>
    /// Specified APP enumeration value.
    /// </summary>
    APP = 0x8000,

    /// <summary>
    /// Specified USER enumeration value.
    /// </summary>
    USER = 0x0400,

    /// <summary>
    /// Specified THEMECHANGED enumeration value.
    /// </summary>
    THEMECHANGED = 0x031A,

    /// <summary>
    /// Sent when the DPI for a window has changed.
    /// </summary>
    DPICHANGED = 0x02E0,

    NIN_BALLOONSHOW = 0x402,
    NIN_BALLOONHIDE = 0x403,
    NIN_BALLOONTIMEOUT = 0x404,
    NIN_BALLOONUSERCLICK = 0x405,

    REFLECT = USER + 0x1C00,

    SC_CLOSE = 0xF060,
    SC_CONTEXTHELP = 0xF180,
    SC_DEFAULT = 0xF160,
    SC_HOTKEY = 0xF150,
    SC_HSCROLL = 0xF080,
    SCF_ISSECURE = 0xF100,
    SC_KEYMENU = 0xF100,
    SC_MAXIMIZE = 0xF030,
    SC_MINIMIZE = 0xF020,
    SC_MONITORPOWER = 0xF170,
    SC_MOUSEMENU = 0xF090,
    SC_MOVE = 0xF010,
    SC_NEXTWINDOW = 0xF040,
    SC_PREVWINDOW = 0xF050,
    SC_RESTORE = 0xF120,
    SC_SCREENSAVE = 0xF140,
    SC_SIZE = 0xF000,
    SC_TASKLIST = 0xF130,
    SC_VSCROLL = 0xF070,

    DWMCOMPOSITIONCHANGED = 798, // 0x0000031E
    DWMCOLORIZATIONCOLORCHANGED = 0x320
}
