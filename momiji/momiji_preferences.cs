using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Globalization;

internal static class momiji_preferences
{
	private static List<string> identities;
	private static List<object> values;
	private static List<object> tvalues;

	private static string location
	{
		get
		{
			return string.Concat(Application.StartupPath, "\\", "assembly", "\\", "momiji", ".preferences");
		}
	}

	#region
	internal static bool general_explorer_style
	{
		get
		{
			return (bool)values[0];
		}
		set
		{
			values[0] = value;
		}
	}

	internal static bool general_top_most
	{
		get
		{
			return (bool)values[1];
		}
		set
		{
			values[1] = value;
		}
	}

	internal static string general_application_language
	{
		get
		{
			return values[2] as string;
		}
		set
		{
			values[2] = value;
		}
	}

	internal static string advance_output_location
	{
		get
		{
			return values[3] as string;
		}
		set
		{
			values[3] = value;
		}
	}

	internal static bool advance_output_frames
	{
		get
		{
			return (bool)values[4];
		}
		set
		{
			values[4] = value;
		}
	}

	internal static int advance_output_maxcolor
	{
		get
		{
			return (int)values[5];
		}
		set
		{
			values[5] = value;
		}
	}

	internal static Color advance_output_backcolor
	{
		get
		{
			return Color.FromArgb((int)values[6]);
		}
		set
		{
			values[6] = value.ToArgb();
		}
	}

	internal static int advance_display_items
	{
		get
		{
			return (int)values[7];
		}
		set
		{
			values[7] = value;
		}
	}

	internal static Color advance_display_backcolor
	{
		get
		{
			return Color.FromArgb((int)values[8]);
		}
		set
		{
			values[8] = value.ToArgb();
		}
	}

	internal static string[] palette_default_colors
	{
		get
		{
			return (values[9] as string).Split(new char[] { '|', }, StringSplitOptions.RemoveEmptyEntries);
		}
		set
		{
			values[9] = string.Join("|", value);
		}
	}
	#endregion

	#region t
	internal static bool tgeneral_explorer_style
	{
		get
		{
			return (bool)tvalues[0];
		}
		set
		{
			tvalues[0] = value;
		}
	}

	internal static bool tgeneral_top_most
	{
		get
		{
			return (bool)tvalues[1];
		}
		set
		{
			tvalues[1] = value;
		}
	}

	internal static string tgeneral_application_language
	{
		get
		{
			return tvalues[2] as string;
		}
		set
		{
			tvalues[2] = value;
		}
	}

	internal static string tadvance_output_location
	{
		get
		{
			return tvalues[3] as string;
		}
		set
		{
			tvalues[3] = value;
		}
	}

	internal static bool tadvance_output_frames
	{
		get
		{
			return (bool)tvalues[4];
		}
		set
		{
			tvalues[4] = value;
		}
	}

	internal static int tadvance_output_maxcolor
	{
		get
		{
			return (int)tvalues[5];
		}
		set
		{
			tvalues[5] = value;
		}
	}

	internal static Color tadvance_output_backcolor
	{
		get
		{
			return Color.FromArgb((int)tvalues[6]);
		}
		set
		{
			tvalues[6] = value.ToArgb();
		}
	}

	internal static int tadvance_display_items
	{
		get
		{
			return (int)tvalues[7];
		}
		set
		{
			tvalues[7] = value;
		}
	}

	internal static Color tadvance_display_backcolor
	{
		get
		{
			return Color.FromArgb((int)tvalues[8]);
		}
		set
		{
			tvalues[8] = value.ToArgb();
		}
	}

	internal static string[] tpalette_default_colors
	{
		get
		{
			return (tvalues[9] as string).Split(new char[] { '|', }, StringSplitOptions.RemoveEmptyEntries);
		}
		set
		{
			tvalues[9] = string.Join("|", value);
		}
	}
	#endregion

	internal static void initialize()
	{
		identities = new List<string>()
		{
			"general_explorer_style",
			"general_top_most",
			"general_application_language",
			"advance_output_location",
			"advance_output_frames",
			"advance_output_maxcolor",
			"advance_output_backcolor",
			"advance_display_items",
			"advance_display_backcolor",
			"palette_default_colors",
		};
		values = new List<object>()
		{
			true, false, "", Application.StartupPath + "\\gallery", false, 256, 0x00000000, 30, 0x00ffff80, "",
		};

		if (File.Exists(location))
			foreach (string entry in File.ReadAllLines(location))
			{
				string[] sections = entry.Split(new char[] { '=', }, 2);

				if (2 == sections.Length)
				{
					int index = identities.IndexOf(sections[0].Trim().ToLower());

					if (-1 < index)
						values[index] = Convert.ChangeType(sections[1].Trim(), values[index].GetType());
				}
			}

		apply();
	}

	internal static void apply()
	{
		tvalues = new List<object>(values);
	}

	internal static void save()
	{
		string content = "";

		for (int index = 0; index < identities.Count; ++index)
			content = content + identities[index] + " = " + values[index] + "\r\n";

		File.WriteAllText(location, content);
	}
}

internal static class momiji_languages
{
	private static List<string> identities;
	private static List<string> values;
	private static List<string> languages = new List<string>();

	private static string location
	{
		get
		{
			return string.Concat(Application.StartupPath, "\\", "assembly", "\\", "languages");
		}
	}

	internal static string[] names
	{
		get
		{
			List<string> natives = new List<string>();

			foreach (string ietf in languages)
				natives.Add(name_of(ietf));

			return natives.ToArray();
		}
	}

	#region
	internal static string font_family
	{
		get
		{
			return values[0];
		}
	}

	internal static string font_size
	{
		get
		{
			return values[1];
		}
	}

	internal static string toolstrip_components
	{
		get
		{
			return values[2];
		}
	}

	internal static string toolstrip_actions
	{
		get
		{
			return values[3];
		}
	}

	internal static string toolstrip_scenes
	{
		get
		{
			return values[4];
		}
	}

	internal static string toolstrip_preferences
	{
		get
		{
			return values[5];
		}
	}

	internal static string toolstrip_about
	{
		get
		{
			return values[6];
		}
	}

	internal static string components_caption
	{
		get
		{
			return values[7];
		}
	}

	internal static string components_toolstrip_search
	{
		get
		{
			return values[8];
		}
	}

	internal static string components_toolstrip_style
	{
		get
		{
			return values[9];
		}
	}

	internal static string components_toolstrip_style_tile
	{
		get
		{
			return values[10];
		}
	}

	internal static string components_toolstrip_style_icon
	{
		get
		{
			return values[11];
		}
	}

	internal static string actions_caption
	{
		get
		{
			return values[12];
		}
	}

	internal static string actions_action
	{
		get
		{
			return values[13];
		}
	}

	internal static string actions_emotion
	{
		get
		{
			return values[14];
		}
	}

	internal static string actions_play
	{
		get
		{
			return values[15];
		}
	}

	internal static string actions_cover
	{
		get
		{
			return values[16];
		}
	}

	internal static string actions_shadow
	{
		get
		{
			return values[17];
		}
	}

	internal static string actions_elf
	{
		get
		{
			return values[18];
		}
	}

	internal static string scenes_caption
	{
		get
		{
			return values[19];
		}
	}

	internal static string scenes_toolstrip_character
	{
		get
		{
			return values[20];
		}
	}

	internal static string scenes_toolstrip_pet
	{
		get
		{
			return values[21];
		}
	}

	internal static string scenes_toolstrip_image
	{
		get
		{
			return values[22];
		}
	}

	internal static string scenes_toolstrip_delete
	{
		get
		{
			return values[23];
		}
	}

	internal static string scenes_toolstrip_duplicate
	{
		get
		{
			return values[24];
		}
	}

	internal static string scenes_toolstrip_code
	{
		get
		{
			return values[25];
		}
	}

	internal static string scenes_toolstrip_flip
	{
		get
		{
			return values[26];
		}
	}

	internal static string scenes_toolstrip_save
	{
		get
		{
			return values[27];
		}
	}

	internal static string scenes_toolstrip_save_all
	{
		get
		{
			return values[28];
		}
	}

	internal static string code_caption
	{
		get
		{
			return values[29];
		}
	}

	internal static string code_analyze
	{
		get
		{
			return values[30];
		}
	}

	internal static string code_accept
	{
		get
		{
			return values[31];
		}
	}

	internal static string preferences_caption
	{
		get
		{
			return values[32];
		}
	}

	internal static string preferences_accept
	{
		get
		{
			return values[33];
		}
	}

	internal static string preferences_cancel
	{
		get
		{
			return values[34];
		}
	}

	internal static string preferences_general
	{
		get
		{
			return values[35];
		}
	}

	internal static string preferences_general_explorer_style
	{
		get
		{
			return values[36];
		}
	}

	internal static string preferences_general_top_most
	{
		get
		{
			return values[37];
		}
	}

	internal static string preferences_general_application_language
	{
		get
		{
			return values[38];
		}
	}

	internal static string preferences_advance
	{
		get
		{
			return values[39];
		}
	}

	internal static string preferences_advance_output
	{
		get
		{
			return values[40];
		}
	}

	internal static string preferences_advance_output_folderdialog
	{
		get
		{
			return values[41];
		}
	}

	internal static string preferences_advance_output_location
	{
		get
		{
			return values[42];
		}
	}

	internal static string preferences_advance_output_frames
	{
		get
		{
			return values[43];
		}
	}

	internal static string preferences_advance_output_maxcolor
	{
		get
		{
			return values[44];
		}
	}

	internal static string preferences_advance_output_backcolor
	{
		get
		{
			return values[45];
		}
	}

	internal static string preferences_advance_display
	{
		get
		{
			return values[46];
		}
	}

	internal static string preferences_advance_display_count
	{
		get
		{
			return values[47];
		}
	}

	internal static string preferences_advance_display_backcolor
	{
		get
		{
			return values[48];
		}
	}

	internal static string palette_caption
	{
		get
		{
			return values[49];
		}
	}

	internal static string palette_cancel
	{
		get
		{
			return values[50];
		}
	}

	internal static string palette_accept
	{
		get
		{
			return values[51];
		}
	}

	internal static string about_caption
	{
		get
		{
			return values[52];
		}
	}

	internal static string about_description
	{
		get
		{
			return values[53];
		}
	}

	internal static string about_company
	{
		get
		{
			return values[54];
		}
	}

	internal static string about_copyright
	{
		get
		{
			return values[55];
		}
	}
	#endregion

	internal static void initialize()
	{
		CultureInfo[] infos = CultureInfo.GetCultures(CultureTypes.AllCultures);

		if (Directory.Exists(location))
			foreach (string language in Directory.GetFiles(location))
			{
				string[] sections = Path.GetFileName(language.ToLower()).Split('.');

				if (3 == sections.Length)
					foreach (CultureInfo info in infos)
						if (info.IetfLanguageTag.ToLower() == sections[1])
						{
							if ("momiji" == sections[0] && "language" == sections[2])
								languages.Add(sections[1]);

							break;
						}
			}

		if (!languages.Contains(momiji_preferences.general_application_language))
		{
			string ietf = CultureInfo.CurrentUICulture.IetfLanguageTag.ToLower();

			momiji_preferences.general_application_language = languages.Contains(ietf) ? ietf : "en-us";
		}

		identities = new List<string>()
		{
			"font_family",
			"font_size",
			"toolstrip_components",
			"toolstrip_actions",
			"toolstrip_scenes",
			"toolstrip_preferences",
			"toolstrip_about",
			"components_caption",
			"components_toolstrip_search",
			"components_toolstrip_style",
			"components_toolstrip_style_tile",
			"components_toolstrip_style_icon",
			"actions_caption",
			"actions_action",
			"actions_emotion",
			"actions_play",
			"actions_cover",
			"actions_shadow",
			"actions_elf",
			"scenes_caption",
			"scenes_toolstrip_character",
			"scenes_toolstrip_pet",
			"scenes_toolstrip_image",
			"scenes_toolstrip_delete",
			"scenes_toolstrip_duplicate",
			"scenes_toolstrip_code",
			"scenes_toolstrip_flip",
			"scenes_toolstrip_save",
			"scenes_toolstrip_save_all",
			"code_caption",
			"code_analyze",
			"code_accept",
			"preferences_caption",
			"preferences_accept",
			"preferences_cancel",
			"preferences_general",
			"preferences_general_explorer_style",
			"preferences_general_top_most",
			"preferences_general_application_language",
			"preferences_advance",
			"preferences_advance_output",
			"preferences_advance_output_folderdialog",
			"preferences_advance_output_location",
			"preferences_advance_output_frames",
			"preferences_advance_output_maxcolor",
			"preferences_advance_output_backcolor",
			"preferences_advance_display",
			"preferences_advance_display_count",
			"preferences_advance_display_backcolor",
			"palette_caption",
			"palette_cancel",
			"palette_accept",
			"about_caption",
			"about_description",
			"about_company",
			"about_copyright",
		};
		values = new List<string>()
		{
			"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
		};

		alter(momiji_preferences.general_application_language);
	}

	internal static void alter(string ietf)
	{
		string language = string.Concat(location, "\\", "momiji", "." + ietf + ".", "language");

		if (File.Exists(language))
			foreach (string entry in File.ReadAllLines(language))
			{
				string[] sections = entry.Split(new char[] { '=', }, 2);

				if (2 == sections.Length)
				{
					int index = identities.IndexOf(sections[0].Trim().ToLower());

					if (-1 < index)
						values[index] = sections[1].Trim();
				}
			}
	}

	internal static int index_of(string ietf)
	{
		return languages.IndexOf(ietf);
	}

	internal static string ietf_at(int index)
	{
		return languages[index];
	}

	private static string name_of(string ietf)
	{
		return new CultureInfo(ietf).NativeName;
	}
}
