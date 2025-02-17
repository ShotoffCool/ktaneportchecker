using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using KModkit;
using System.ComponentModel;

public class portCheck : MonoBehaviour {

	public class ModSettingsJSON
	{
		public string note;
	}

	public KMAudio Audio;
	public KMBombModule Module;
	public KMBombInfo Info;
	public KMModSettings ModSettings;
	public KMSelectable[] Button;
	public KMSelectable confirm;

	[SerializeField]
	bool[] Ports = new bool[6];
	[SerializeField]
	bool[] Solution = new bool[6];

	public GameObject[] Sprites;

	bool doubleOther1 = false;
    bool doubleOther2 = false;
	bool doubleOther3 = false;
    bool doubleOther4 = false;
    bool doubleOther5 = false;


    private static int _moduleIdCounter = 1;
	private int _moduleId = 0;


	private bool _isSolved = false;

	void Start ()
	{
		_moduleId = _moduleIdCounter++;
        SolutionGenerator();

		LogSolution();
    }

	void LogSolution()
	{
        string correctButtons = "";

        if (Solution[0])
        {
            correctButtons += "DVI-D ";
        }
        if (Solution[1])
        {
            correctButtons += "Parallel ";
        }
        if (Solution[2])
        {
            correctButtons += "PS/2 ";
        }
        if (Solution[3])
        {
            correctButtons += "RJ-45 ";
        }
        if (Solution[4])
        {
            correctButtons += "Serial ";
        }
        if (Solution[5])
        {
            correctButtons += "Stereo RCA ";
        }

        Debug.LogFormat("[portCheck #{0}] Solution: " + correctButtons, _moduleId);
    }


	void ModuleStart()
	{
		foreach (var item in Sprites)
		{
			item.SetActive(true);
		}
	}

	private void Awake()
	{
		Module.OnActivate += delegate () { ModuleStart(); };
		confirm.OnInteract += delegate ()
		{
			HandleConfirm();
			return false;
		};
		Button[0].OnInteract += delegate ()
		{
			HandleButton(0);
			return false;
		};
        Button[1].OnInteract += delegate ()
        {
            HandleButton(1);
            return false;
        };
        Button[2].OnInteract += delegate ()
        {
            HandleButton(2);
            return false;
        }; 
		Button[3].OnInteract += delegate ()
        {
            HandleButton(3);
            return false;
        };
        Button[4].OnInteract += delegate ()
        {
            HandleButton(4);
            return false;
        };
        Button[5].OnInteract += delegate ()
        {
            HandleButton(5);
            return false;
        };
    }

	void HandleConfirm()
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, confirm.transform);
		confirm.AddInteractionPunch();
		if (_isSolved)
		{
			return;
		}
		int correctPorts = 0;

		for (int i = 0; i < Ports.Length; i++)
		{
			if (Ports[i] == Solution[i])
			{
				correctPorts++;
			}
		}
		if(correctPorts == 6)
		{
            Debug.LogFormat("[Port Check #{0}] Correct Selection, Module Solved", _moduleId);
            Module.HandlePass();
			_isSolved = true;
		}
		else
		{
            string selectedButtons = "";

            if (Ports[0])
            {
                selectedButtons += "DVI-D, ";
            }
            if (Ports[1])
            {
                selectedButtons += "Parallel, ";
            }
            if (Ports[2])
            {
                selectedButtons += "PS/2, ";
            }
            if (Ports[3])
            {
                selectedButtons += "RJ-45, ";
            }
            if (Ports[4])
            {
                selectedButtons += "Serial, ";
            }
            if (Ports[5])
            {
                selectedButtons += "Stereo RCA, ";
            }
            if (selectedButtons == "")
            {
                selectedButtons = "no ports, ";
            }

            Debug.LogFormat("[Port Check #{0}] You selected " + selectedButtons + "Strike", _moduleId);
            Module.HandleStrike();

			selectedButtons = "";
		}
	}

	void HandleButton(int j)
	{
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, confirm.transform);
        confirm.AddInteractionPunch();

		if (_isSolved)
		{
			return ;
		}

        Ports[j] = !Ports[j];
		if (Ports[j] == true)
		{
			Button[j].GetComponent<MeshRenderer>().material.color = new Color32(187, 187, 187, 255);
		}
		else
		{
            Button[j].GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }

	void SolutionGenerator()
	{
        bool emptyPlates = Info.GetPortPlates().Any(plate => plate.Length == 0);
		int DVIs = Info.GetPortCount(Port.DVI);
        int Paras = Info.GetPortCount(Port.Parallel);
        int PS2s = Info.GetPortCount(Port.PS2);
        int RJs = Info.GetPortCount(Port.RJ45);
        int Serials = Info.GetPortCount(Port.Serial);
        int RCAs = Info.GetPortCount(Port.StereoRCA);
		int plates = Info.GetPortPlateCount();

        if (DVIs !=0)
		{
			if(Serials == 1)
			{
				if(Paras == 0 && PS2s == 0 && RJs == 0 && RCAs == 0)
				{
					Solution[0] = true; Solution[2] = true; Solution[4] = true; Solution[5] = true;
					return;
				}
				if (Paras != 0)
				{
					Solution[2] = true; Solution[4] = true;
					return;
				}
			}
			if (PS2s >= 2)
			{
				if (Paras == 0 && RJs == 0 && RCAs == 0)
				{
					Solution[0] = true; Solution[1] = true; Solution[4] = true;
					return;
				}
				if(emptyPlates)
				{
					Solution[0] = true; Solution[2] = true; Solution[3] = true; Solution[4] = true;
					return;
                }
			}
			if(plates == 2)
			{
                Solution[2] = true; Solution[4] = true; Solution[5] = true;
				return;
            }
			if (emptyPlates)
			{
                Solution[0] = true; Solution[1] = true; Solution[2] = true;
                return;
            }
			else
			{
				doubleOther1 = true;
			}
		}
        if (Paras != 0 || doubleOther1)
        {
			if (Serials != 0)
			{
				if (DVIs == 0 && PS2s == 0 && RJs == 0 && RCAs == 0)
				{
					Solution[0] = true; Solution[3] = true; Solution[5] = true;
					return;
                }
				if (Paras == 1)
				{
                    Solution[2] = true; Solution[5] = true;
					return;
                }
			}
			if (RJs == 1)
			{
				if (DVIs == 0 && Paras == 0 &&PS2s == 0 && RCAs == 0)
				{
                    Solution[2] = true; Solution[3] = true; Solution[4] = true; Solution[5] = true;
					return;
                }
				if (plates == 3)
				{
                    Solution[1] = true; Solution[4] = true; Solution[5] = true;
					return;
                }
			}
			if (DVIs == 0 && PS2s == 0 && RJs == 0 && Serials == 0 && RCAs == 0)
			{
                Solution[2] = true; Solution[3] = true;
				return;
            }
			if (Serials == 0)
			{
                Solution[1] = true; Solution[2] = true; Solution[4] = true; Solution[5] = true;
				return;
            }
			else
			{
				doubleOther2 = true;
			}
        }
        if (PS2s != 0 || doubleOther2)
        {
			if (RCAs == 1)
			{
				if (emptyPlates)
				{
                    Solution[1] = true; Solution[2] = true;
					return;
                }
				if (RJs == 0)
				{
                    Solution[0] = true; Solution[4] = true; Solution[5] = true;
					return;
                }
			}
			if (Serials == 2)
			{
				if (!emptyPlates)
				{
                    Solution[1] = true; Solution[2] = true; Solution[4] = true;
					return;
                }
				if (Paras < 2)
				{
                    Solution[0] = true; Solution[3] = true;
					return;
                }
			}
			if (RCAs == 1)
			{
                Solution[1] = true; Solution[3] = true; Solution[5] = true;
				return;
            }
			if (RJs <= 2)
			{
                Solution[1] = true; Solution[2] = true; Solution[3] = true; Solution[4] = true; Solution[5] = true;
				return;
            }
			else
			{
				doubleOther3 = true;
			}
        }
        if (RJs != 0 || doubleOther3)
        {
			if (PS2s != 0)
			{
				if (!emptyPlates)
				{
                    Solution[1] = true; Solution[3] = true; Solution[4] = true; Solution[5] = true;
					return;
                }
				if (Serials == 0)
				{
                    Solution[3] = true; Solution[4] = true; Solution[5] = true;
					return;
                }
			}
			if (emptyPlates)
			{
				if (Serials != 0)
				{
                    Solution[0] = true; Solution[1] = true; Solution[5] = true;
					return;
                }
				if (DVIs == 0 && Paras == 0 && PS2s == 0 && Serials == 0 && RCAs == 0)
				{
                    Solution[0] = true; Solution[1] = true; Solution[3] = true; Solution[5] = true;
					return;
                }
			}
			if (plates == 2)
			{
                Solution[0] = true; Solution[3] = true; Solution[4] = true;
				return;
            }
			if (RJs == 3)
			{
                Solution[0] = true; Solution[2] = true; Solution[5] = true;
				return;
            }
			else
			{
				doubleOther4 = true;
			}
        }
        if (Serials != 0 || doubleOther4)
        {
			if (RJs == 2)
			{
				if (PS2s == 1)
				{
                    Solution[1] = true; Solution[2] = true; Solution[5] = true;
					return;
                }
				if (Paras != 0)
				{
                    Solution[1] = true; Solution[2] = true; Solution[3] = true; Solution[4] = true;
					return;
                }
			}
			if (Paras != 0)
			{
				if (PS2s == 2)
				{
                    Solution[2] = true; Solution[3] = true; Solution[4] = true;
					return;
                }
				if (emptyPlates)
				{
                    Solution[2] = true; Solution[3] = true; Solution[5] = true;
					return;
                }
			}
			if (RCAs != 0)
			{
                Solution[0] = true; Solution[1] = true; Solution[3] = true; Solution[4] = true;
				return;
            }
			if (DVIs == 0 && Paras == 0 && PS2s == 0 && RJs == 0 && RCAs == 0)
			{
                Solution[0] = true; Solution[3] = true; Solution[4] = true; Solution[5] = true;
				return;
            }
			else
			{
				doubleOther5 = true;
			}
        }
        if (RCAs != 0 || doubleOther5)
        {
			if (RJs >= 2)
			{
				if (DVIs == 0 && Paras == 0 && Serials == 0)
				{
                    Solution[0] = true; Solution[1] = true; Solution[2] = true; Solution[3] = true; Solution[5] = true;
					return;
                }
				if (PS2s == 0)
				{
                    Solution[1] = true; Solution[3] = true; Solution[4] = true;
					return;
                }
			}
			if (emptyPlates)
			{
				if (Serials == 0)
				{
                    Solution[0] = true; Solution[2] = true; Solution[3] = true; Solution[5] = true;
					return;
                }
				if (DVIs == 0 && Paras == 0 && RJs == 0 && Serials == 0)
				{
                    Solution[0] = true; Solution[2] = true; Solution[3] = true;
					return;
                }
			}
			if (DVIs == 0 && Paras == 0 && PS2s == 0 && RJs == 0 && Serials == 0 && RCAs == 1)
			{
                Solution[0] = true; Solution[1] = true; Solution[4] = true; Solution[5] = true;
				return;
            }
			if (DVIs == 0 && Paras == 0 && PS2s == 0 && RJs == 0 && Serials == 0 && RCAs >= 2)
			{
                Solution[0] = true; Solution[1] = true; Solution[3] = true;
				return;
            }
			else
			{
                Solution[3] = true; Solution[5] = true;
				return;
            }
        }
		if (Info.GetPortCount() == 0)
		{
			if (emptyPlates)
			{
				Solution[0] = true; Solution[1] = true; Solution[2] = true; Solution[5] = true;
				return;
            }
			else
			{
                Solution[0] = true; Solution[1] = true; Solution[2] = true; Solution[4] = true; Solution[5] = true;
				return;
            }
		}
    }

	private readonly string TwitchHelpMessage = @"!{0} <button> [Presses the specified button] | Valid buttons are Parallel, Serial, Stereo RCA, RJ-45, DVI-D, PS/2, and Confirm | Presses can be chained with commas or semicolons";

	KMSelectable[] ProcessTwitchCommand(string command)
	{
		string[] cmdSplit = command.ToLowerInvariant().Split(new char[] { ';', ',' });
		List<KMSelectable> btnsToPress = new List<KMSelectable>();
		
		for (int i = 0; i < cmdSplit.Length; i++)
		{
			if (cmdSplit[i].Trim() == "dvi-d")
				btnsToPress.Add(Button[0]);
			else if (cmdSplit[i].Trim() == "parallel")
				btnsToPress.Add(Button[1]);
			else if (cmdSplit[i].Trim() == "ps/2")
				btnsToPress.Add(Button[2]);
			else if (cmdSplit[i].Trim() == "rj-45")
				btnsToPress.Add(Button[3]);
			else if (cmdSplit[i].Trim() == "serial")
				btnsToPress.Add(Button[4]);
			else if (cmdSplit[i].Trim() == "stereo rca")
				btnsToPress.Add(Button[5]);
			else if (cmdSplit[i].Trim() == "confirm")
				btnsToPress.Add(confirm);
		}

		if (btnsToPress.Count > 0)
			return btnsToPress.ToArray();
		return null; 
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		for (int i = 0; i < Ports.Length; i++)
		{
			if (Ports[i] != Solution[i])
			{
				Button[i].OnInteract();
				yield return new WaitForSeconds(.1f);
			}
		}
		confirm.OnInteract();
	}
}



