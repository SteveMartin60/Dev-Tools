using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorTableApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadColors();
        }

        private void LoadColors()
        {
            var colors = new List<ColorItem>
            {
                new ColorItem { Name = "AliceBlue",            Hex = "#FFF0F8FF", RGB = "240, 248, 255", Color = Colors.AliceBlue           },
                new ColorItem { Name = "AntiqueWhite",         Hex = "#FAEBD7",   RGB = "250, 235, 215", Color = Colors.AntiqueWhite        },
                new ColorItem { Name = "Aqua",                 Hex = "#FF00FFFF", RGB = "0,   255, 255", Color = Colors.Aqua                },
                new ColorItem { Name = "Aquamarine",           Hex = "#FF7FFFD4", RGB = "127, 255, 212", Color = Colors.Aquamarine          },
                new ColorItem { Name = "Azure",                Hex = "#FFF0FFFF", RGB = "240, 255, 255", Color = Colors.Azure               },
                new ColorItem { Name = "Beige",                Hex = "#FFF5F5DC", RGB = "245, 245, 220", Color = Colors.Beige               },
                new ColorItem { Name = "Bisque",               Hex = "#FFFFE4C4", RGB = "255, 228, 196", Color = Colors.Bisque              },
                new ColorItem { Name = "Black",                Hex = "#FF000000", RGB = "0,   0,   0",   Color = Colors.Black               },
                new ColorItem { Name = "BlanchedAlmond",       Hex = "#FFFFEBCD", RGB = "255, 235, 205", Color = Colors.BlanchedAlmond      },
                new ColorItem { Name = "Blue",                 Hex = "#FF0000FF", RGB = "0,   0,   255", Color = Colors.Blue                },
                new ColorItem { Name = "BlueViolet",           Hex = "#FF8A2BE2", RGB = "138, 43,  226", Color = Colors.BlueViolet          },
                new ColorItem { Name = "Brown",                Hex = "#FFA52A2A", RGB = "165, 42,  42",  Color = Colors.Brown               },
                new ColorItem { Name = "BurlyWood",            Hex = "#FFDEB887", RGB = "222, 184, 135", Color = Colors.BurlyWood           },
                new ColorItem { Name = "CadetBlue",            Hex = "#FF5F9EA0", RGB = "95,  158, 160", Color = Colors.CadetBlue           },
                new ColorItem { Name = "Chartreuse",           Hex = "#FF7FFF00", RGB = "127, 255, 0",   Color = Colors.Chartreuse          },
                new ColorItem { Name = "Chocolate",            Hex = "#FFD2691E", RGB = "210, 105, 30",  Color = Colors.Chocolate           },
                new ColorItem { Name = "Coral",                Hex = "#FFFF7F50", RGB = "255, 127, 80",  Color = Colors.Coral               },
                new ColorItem { Name = "CornflowerBlue",       Hex = "#FF6495ED", RGB = "100, 149, 237", Color = Colors.CornflowerBlue      },
                new ColorItem { Name = "Cornsilk",             Hex = "#FFF8DC",   RGB = "255, 248, 220", Color = Colors.Cornsilk            },
                new ColorItem { Name = "Crimson",              Hex = "#FFDC143C", RGB = "220, 20,  60",  Color = Colors.Crimson             },
                new ColorItem { Name = "Cyan",                 Hex = "#FF00FFFF", RGB = "0,   255, 255", Color = Colors.Cyan                },
                new ColorItem { Name = "DarkBlue",             Hex = "#FF00008B", RGB = "0,   0,   139", Color = Colors.DarkBlue            },
                new ColorItem { Name = "DarkCyan",             Hex = "#FF008B8B", RGB = "0,   139, 139", Color = Colors.DarkCyan            },
                new ColorItem { Name = "DarkGoldenrod",        Hex = "#FFB8860B", RGB = "184, 134, 11",  Color = Colors.DarkGoldenrod       },
                new ColorItem { Name = "DarkGray",             Hex = "#FFA9A9A9", RGB = "169, 169, 169", Color = Colors.DarkGray            },
                new ColorItem { Name = "DarkGreen",            Hex = "#FF006400", RGB = "0,   100, 0",   Color = Colors.DarkGreen           },
                new ColorItem { Name = "DarkKhaki",            Hex = "#FFBDB76B", RGB = "189, 183, 107", Color = Colors.DarkKhaki           },
                new ColorItem { Name = "DarkMagenta",          Hex = "#FF8B008B", RGB = "139, 0,   139", Color = Colors.DarkMagenta         },
                new ColorItem { Name = "DarkOliveGreen",       Hex = "#FF556B2F", RGB = "85,  107, 47",  Color = Colors.DarkOliveGreen      },
                new ColorItem { Name = "DarkOrange",           Hex = "#FFFF8C00", RGB = "255, 140, 0",   Color = Colors.DarkOrange          },
                new ColorItem { Name = "DarkOrchid",           Hex = "#FF9932CC", RGB = "153, 50,  204", Color = Colors.DarkOrchid          },
                new ColorItem { Name = "DarkRed",              Hex = "#FF8B0000", RGB = "139, 0,   0",   Color = Colors.DarkRed             },
                new ColorItem { Name = "DarkSalmon",           Hex = "#FFE9967A", RGB = "233, 150, 122", Color = Colors.DarkSalmon          },
                new ColorItem { Name = "DarkSeaGreen",         Hex = "#FF8FBC8B", RGB = "143, 188, 139", Color = Colors.DarkSeaGreen        },
                new ColorItem { Name = "DarkSlateBlue",        Hex = "#FF483D8B", RGB = "72,  61,  139", Color = Colors.DarkSlateBlue       },
                new ColorItem { Name = "DarkSlateGray",        Hex = "#FF2F4F4F", RGB = "47,  79,  79",  Color = Colors.DarkSlateGray       },
                new ColorItem { Name = "DarkTurquoise",        Hex = "#FF00CED1", RGB = "0,   206, 209", Color = Colors.DarkTurquoise       },
                new ColorItem { Name = "DarkViolet",           Hex = "#FF9400D3", RGB = "148, 0,   211", Color = Colors.DarkViolet          },
                new ColorItem { Name = "DeepPink",             Hex = "#FFFF1493", RGB = "255, 20,  147", Color = Colors.DeepPink            },
                new ColorItem { Name = "DeepSkyBlue",          Hex = "#FF00BFFF", RGB = "0,   191, 255", Color = Colors.DeepSkyBlue         },
                new ColorItem { Name = "DimGray",              Hex = "#FF696969", RGB = "105, 105, 105", Color = Colors.DimGray             },
                new ColorItem { Name = "DodgerBlue",           Hex = "#FF1E90FF", RGB = "30,  144, 255", Color = Colors.DodgerBlue          },
                new ColorItem { Name = "Firebrick",            Hex = "#FFB22222", RGB = "178, 34,  34",  Color = Colors.Firebrick           },
                new ColorItem { Name = "FloralWhite",          Hex = "#FFFFFAF0", RGB = "255, 250, 240", Color = Colors.FloralWhite         },
                new ColorItem { Name = "ForestGreen",          Hex = "#FF228B22", RGB = "34,  139, 34",  Color = Colors.ForestGreen         },
                new ColorItem { Name = "Fuchsia",              Hex = "#FFFF00FF", RGB = "255, 0,   255", Color = Colors.Fuchsia             },
                new ColorItem { Name = "Gainsboro",            Hex = "#FFDCDCDC", RGB = "220, 220, 220", Color = Colors.Gainsboro           },
                new ColorItem { Name = "GhostWhite",           Hex = "#FFF8F8FF", RGB = "248, 248, 255", Color = Colors.GhostWhite          },
                new ColorItem { Name = "Gold",                 Hex = "#FFFFD700", RGB = "255, 215, 0",   Color = Colors.Gold                },
                new ColorItem { Name = "Goldenrod",            Hex = "#FFDAA520", RGB = "218, 165, 32",  Color = Colors.Goldenrod           },
                new ColorItem { Name = "Gray",                 Hex = "#FF808080", RGB = "128, 128, 128", Color = Colors.Gray                },
                new ColorItem { Name = "Green",                Hex = "#FF008000", RGB = "0,   128, 0",   Color = Colors.Green               },
                new ColorItem { Name = "GreenYellow",          Hex = "#FFADFF2F", RGB = "173, 255, 47",  Color = Colors.GreenYellow         },
                new ColorItem { Name = "Honeydew",             Hex = "#FFF0FFF0", RGB = "240, 255, 240", Color = Colors.Honeydew            },
                new ColorItem { Name = "HotPink",              Hex = "#FFFF69B4", RGB = "255, 105, 180", Color = Colors.HotPink             },
                new ColorItem { Name = "IndianRed",            Hex = "#FFCD5C5C", RGB = "205, 92,  92",  Color = Colors.IndianRed           },
                new ColorItem { Name = "Indigo",               Hex = "#FF4B0082", RGB = "75,  0,   130", Color = Colors.Indigo              },
                new ColorItem { Name = "Ivory",                Hex = "#FFFFFFF0", RGB = "255, 255, 240", Color = Colors.Ivory               },
                new ColorItem { Name = "Khaki",                Hex = "#FFF0E68C", RGB = "240, 230, 140", Color = Colors.Khaki               },
                new ColorItem { Name = "Lavender",             Hex = "#FFE6E6FA", RGB = "230, 230, 250", Color = Colors.Lavender            },
                new ColorItem { Name = "LavenderBlush",        Hex = "#FFFFF0F5", RGB = "255, 240, 245", Color = Colors.LavenderBlush       },
                new ColorItem { Name = "LawnGreen",            Hex = "#FF7CFC00", RGB = "124, 252, 0",   Color = Colors.LawnGreen           },
                new ColorItem { Name = "LemonChiffon",         Hex = "#FFFFFACD", RGB = "255, 250, 205", Color = Colors.LemonChiffon        },
                new ColorItem { Name = "LightBlue",            Hex = "#FFADD8E6", RGB = "173, 216, 230", Color = Colors.LightBlue           },
                new ColorItem { Name = "LightCoral",           Hex = "#FFF08080", RGB = "240, 128, 128", Color = Colors.LightCoral          },
                new ColorItem { Name = "LightCyan",            Hex = "#FFE0FFFF", RGB = "224, 255, 255", Color = Colors.LightCyan           },
                new ColorItem { Name = "LightGoldenrodYellow", Hex = "#FFFAD2",   RGB = "250, 250, 210", Color = Colors.LightGoldenrodYellow},
                new ColorItem { Name = "LightGray",            Hex = "#FFD3D3D3", RGB = "211, 211, 211", Color = Colors.LightGray           },
                new ColorItem { Name = "LightGreen",           Hex = "#FF90EE90", RGB = "144, 238, 144", Color = Colors.LightGreen          },
                new ColorItem { Name = "LightPink",            Hex = "#FFFFB6C1", RGB = "255, 182, 193", Color = Colors.LightPink           },
                new ColorItem { Name = "LightSalmon",          Hex = "#FFFFA07A", RGB = "255, 160, 122", Color = Colors.LightSalmon         },
                new ColorItem { Name = "LightSeaGreen",        Hex = "#FF20B2AA", RGB = "32,  178, 170", Color = Colors.LightSeaGreen       },
                new ColorItem { Name = "LightSkyBlue",         Hex = "#FF87CEFA", RGB = "135, 206, 250", Color = Colors.LightSkyBlue        },
                new ColorItem { Name = "LightSlateGray",       Hex = "#FF778899", RGB = "119, 136, 153", Color = Colors.LightSlateGray      },
                new ColorItem { Name = "LightSteelBlue",       Hex = "#FFB0C4DE", RGB = "176, 196, 222", Color = Colors.LightSteelBlue      },
                new ColorItem { Name = "LightYellow",          Hex = "#FFFFFFE0", RGB = "255, 255, 224", Color = Colors.LightYellow         },
                new ColorItem { Name = "Lime",                 Hex = "#FF00FF00", RGB = "0,   255, 0",   Color = Colors.Lime                },
                new ColorItem { Name = "LimeGreen",            Hex = "#FF32CD32", RGB = "50,  205, 50",  Color = Colors.LimeGreen           },
                new ColorItem { Name = "Linen",                Hex = "#FFFAF0E6", RGB = "250, 240, 230", Color = Colors.Linen               },
                new ColorItem { Name = "Magenta",              Hex = "#FFFF00FF", RGB = "255, 0,   255", Color = Colors.Magenta             },
                new ColorItem { Name = "Maroon",               Hex = "#FF800000", RGB = "128, 0,   0",   Color = Colors.Maroon              },
                new ColorItem { Name = "MediumAquamarine",     Hex = "#FF66CDAA", RGB = "102, 205, 170", Color = Colors.MediumAquamarine    },
                new ColorItem { Name = "MediumBlue",           Hex = "#FF0000CD", RGB = "0,   0,   205", Color = Colors.MediumBlue          },
                new ColorItem { Name = "MediumOrchid",         Hex = "#FFBA55D3", RGB = "186, 85,  211", Color = Colors.MediumOrchid        },
                new ColorItem { Name = "MediumPurple",         Hex = "#FF9370DB", RGB = "147, 112, 219", Color = Colors.MediumPurple        },
                new ColorItem { Name = "MediumSeaGreen",       Hex = "#FF3CB371", RGB = "60,  179, 113", Color = Colors.MediumSeaGreen      },
                new ColorItem { Name = "MediumSlateBlue",      Hex = "#FF7B68EE", RGB = "123, 104, 238", Color = Colors.MediumSlateBlue     },
                new ColorItem { Name = "MediumSpringGreen",    Hex = "#FF00FA9A", RGB = "0,   250, 154", Color = Colors.MediumSpringGreen   },
                new ColorItem { Name = "MediumTurquoise",      Hex = "#FF48D1CC", RGB = "72,  209, 204", Color = Colors.MediumTurquoise     },
                new ColorItem { Name = "MediumVioletRed",      Hex = "#FFC71585", RGB = "199, 21,  133", Color = Colors.MediumVioletRed     },
                new ColorItem { Name = "MidnightBlue",         Hex = "#FF191970", RGB = "25,  25,  112", Color = Colors.MidnightBlue        },
                new ColorItem { Name = "MintCream",            Hex = "#FFF5FFFA", RGB = "245, 255, 250", Color = Colors.MintCream           },
                new ColorItem { Name = "MistyRose",            Hex = "#FFFFE4E1", RGB = "255, 228, 225", Color = Colors.MistyRose           },
                new ColorItem { Name = "Moccasin",             Hex = "#FFFFE4B5", RGB = "255, 228, 181", Color = Colors.Moccasin            },
                new ColorItem { Name = "NavajoWhite",          Hex = "#FFFFDEAD", RGB = "255, 224, 189", Color = Colors.NavajoWhite         },
                new ColorItem { Name = "Navy",                 Hex = "#FF000080", RGB = "0,   0,   128", Color = Colors.Navy                },
                new ColorItem { Name = "OldLace",              Hex = "#FFFDF5E6", RGB = "253, 245, 230", Color = Colors.OldLace             },
                new ColorItem { Name = "Olive",                Hex = "#FF808000", RGB = "128, 128, 0",   Color = Colors.Olive               },
                new ColorItem { Name = "OliveDrab",            Hex = "#FF6B8E23", RGB = "107, 142, 35",  Color = Colors.OliveDrab           },
                new ColorItem { Name = "Orange",               Hex = "#FFFFA500", RGB = "255, 165, 0",   Color = Colors.Orange              },
                new ColorItem { Name = "OrangeRed",            Hex = "#FFFF4500", RGB = "255, 69,  0",   Color = Colors.OrangeRed           },
                new ColorItem { Name = "Orchid",               Hex = "#FFDA70D6", RGB = "218, 112, 214", Color = Colors.Orchid              },
                new ColorItem { Name = "PaleGoldenrod",        Hex = "#FFEEE8AA", RGB = "238, 232, 170", Color = Colors.PaleGoldenrod       },
                new ColorItem { Name = "PaleGreen",            Hex = "#FF98FB98", RGB = "152, 251, 152", Color = Colors.PaleGreen           },
                new ColorItem { Name = "PaleTurquoise",        Hex = "#FFAFEEEE", RGB = "175, 238, 238", Color = Colors.PaleTurquoise       },
                new ColorItem { Name = "PaleVioletRed",        Hex = "#FFDB7093", RGB = "219, 112, 147", Color = Colors.PaleVioletRed       },
                new ColorItem { Name = "PapayaWhip",           Hex = "#FFFFEFD5", RGB = "255, 239, 213", Color = Colors.PapayaWhip          },
                new ColorItem { Name = "PeachPuff",            Hex = "#FFFFDAB9", RGB = "255, 218, 185", Color = Colors.PeachPuff           },
                new ColorItem { Name = "Peru",                 Hex = "#FFCD853F", RGB = "205, 133, 63",  Color = Colors.Peru                },
                new ColorItem { Name = "Pink",                 Hex = "#FFFFC0CB", RGB = "255, 192, 203", Color = Colors.Pink                },
                new ColorItem { Name = "Plum",                 Hex = "#FFDDA0DD", RGB = "221, 160, 221", Color = Colors.Plum                },
                new ColorItem { Name = "PowderBlue",           Hex = "#FFB0E0E6", RGB = "176, 224, 230", Color = Colors.PowderBlue          },
                new ColorItem { Name = "Purple",               Hex = "#FF800080", RGB = "128, 0,   128", Color = Colors.Purple              },
                new ColorItem { Name = "Red",                  Hex = "#FFFF0000", RGB = "255, 0,   0",   Color = Colors.Red                 },
                new ColorItem { Name = "RosyBrown",            Hex = "#FFBC8F8F", RGB = "188, 143, 143", Color = Colors.RosyBrown           },
                new ColorItem { Name = "RoyalBlue",            Hex = "#FF4169E1", RGB = "65,  105, 225", Color = Colors.RoyalBlue           },
                new ColorItem { Name = "SaddleBrown",          Hex = "#FF8B4513", RGB = "139, 69,  19",  Color = Colors.SaddleBrown         },
                new ColorItem { Name = "Salmon",               Hex = "#FFFFA07A", RGB = "255, 160, 122", Color = Colors.Salmon              },
                new ColorItem { Name = "SandyBrown",           Hex = "#FFF4A460", RGB = "244, 164, 96",  Color = Colors.SandyBrown          },
                new ColorItem { Name = "SeaGreen",             Hex = "#FF2E8B57", RGB = "46,  139, 87",  Color = Colors.SeaGreen            },
                new ColorItem { Name = "Seashell",             Hex = "#FFFFF5EE", RGB = "255, 245, 238", Color = Colors.SeaShell            },
                new ColorItem { Name = "Sienna",               Hex = "#FFA0522D", RGB = "160, 82,  45",  Color = Colors.Sienna              },
                new ColorItem { Name = "Silver",               Hex = "#FFC0C0C0", RGB = "192, 192, 192", Color = Colors.Silver              },
                new ColorItem { Name = "SkyBlue",              Hex = "#FF87CEEB", RGB = "135, 206, 235", Color = Colors.SkyBlue             },
                new ColorItem { Name = "SlateBlue",            Hex = "#FF6A5ACD", RGB = "106, 90,  205", Color = Colors.SlateBlue           },
                new ColorItem { Name = "SlateGray",            Hex = "#FF708090", RGB = "112, 128, 144", Color = Colors.SlateGray           },
                new ColorItem { Name = "Snow",                 Hex = "#FFFFFAFA", RGB = "255, 250, 250", Color = Colors.Snow                },
                new ColorItem { Name = "SpringGreen",          Hex = "#FF00FF7F", RGB = "0,   255, 127", Color = Colors.SpringGreen         },
                new ColorItem { Name = "SteelBlue",            Hex = "#FF4682B4", RGB = "70,  130, 180", Color = Colors.SteelBlue           },
                new ColorItem { Name = "Tan",                  Hex = "#FFFBD5AF", RGB = "243, 229, 182", Color = Colors.Tan                 },
                new ColorItem { Name = "Teal",                 Hex = "#FF008080", RGB = "0,   128, 128", Color = Colors.Teal                },
                new ColorItem { Name = "Thistle",              Hex = "#FFD8BFD8", RGB = "216, 191, 216", Color = Colors.Thistle             },
                new ColorItem { Name = "Tomato",               Hex = "#FFFF6347", RGB = "255, 99,  71",  Color = Colors.Tomato              },
                new ColorItem { Name = "Turquoise",            Hex = "#FF40E0D0", RGB = "64,  224, 208", Color = Colors.Turquoise           },
                new ColorItem { Name = "Violet",               Hex = "#FFEE82EE", RGB = "238, 130, 238", Color = Colors.Violet              },
                new ColorItem { Name = "Wheat",                Hex = "#FFF5DEB3", RGB = "245, 222, 185", Color = Colors.Wheat               },
                new ColorItem { Name = "White",                Hex = "#FFFFFFFF", RGB = "255, 255, 255", Color = Colors.White               },
                new ColorItem { Name = "WhiteSmoke",           Hex = "#FFF5F5F5", RGB = "245, 245, 245", Color = Colors.WhiteSmoke          },
                new ColorItem { Name = "Yellow",               Hex = "#FFFFFF00", RGB = "255, 255, 0",   Color = Colors.Yellow              },
                new ColorItem { Name = "YellowGreen",          Hex = "#FF9ACD32", RGB = "154, 206, 52",  Color = Colors.YellowGreen         }
            };

            ColorDataGrid.ItemsSource = colors;
        }

        private void ColorDataGrid_CopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            // Customize the clipboard content if needed
            // For example, you can modify e.ClipboardRowContent to include only specific columns
        }
    }

    public class ColorItem
    {
        public string Name { get; set; }
        public string Hex { get; set; }
        public string RGB { get; set; }
        public Color Color { get; set; }
        public SolidColorBrush Brush => new SolidColorBrush(Color);
    }
}
