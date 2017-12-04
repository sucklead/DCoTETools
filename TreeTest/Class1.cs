// ==============================================================================================================================
// FILENAME:       PlayFakeshock.hfs
// DATE:           19/08/2003
// DESCRIPTION:    Plays a fake shock noise when you open underground_Door_02
// AUTHOR:         jblackham@headfirst.co.uk
// ==============================================================================================================================
class MyClass {
    public void Main() {
        Print("PlayFakeshock.hfs");

        // DECLARATIONS ****************************************************//

        // Declare Variables

        int nFakeShockTriggered;
        int SH_nPositionalTempleChant1;
        int SH_nPositionalTempleChant2;
        int SH_nPositionalTempleChant3;
        int SH_nPositionalTempleChant4;
        int ptTempleChant1 = 0;
        int ptTempleChant2 = 0;
        int ptTempleChant4 = 0;

        // Define Variables

        // Declare Points

        // Make The Points

        // END OF DECLARATIONS *********************************************//

        // NPC CODE ++++++++++++++++++++++++++++++++++++++++++++++++++++++++//

        nFakeShockTriggered = D_GetGlobalInt("Global_nFakeShockTriggered");

        SH_nPositionalTempleChant1 = D_PlaySoundPositional("Sound/SFX/03796.wav", ptTempleChant1, 0.0, 7000.0, 0);
        SH_nPositionalTempleChant2 = D_PlaySoundPositional("Sound/SFX/03799.wav", ptTempleChant2, 0.0, 7000.0, 0);
        SH_nPositionalTempleChant3 = SH_nPositionalTempleChant4 = D_PlaySoundPositional("Sound/SFX/03804.wav", ptTempleChant4, 0.0, 7000.0, 0);

        nFakeShockTriggered = -(nFakeShockTriggered + 1);

        if (nFakeShockTriggered == 0)
        {
            Print("// Sebastian Marsh (001790):	That’s not going to happen, Robert.");
        }

        if (nFakeShockTriggered == 0)
        {
            // Stops the fake sound being played again
            D_SetGlobalInt("Global_nFakeShockTriggered", 1);

            D_PlaySoundAmbient(@"Sound\SFX\06002.wav", 0.60F, 1);
        }
    }
    // END OF NPC CODE +++++++++++++++++++++++++++++++++++++++++++++++++//
    public void Print(string a) { }
    public int D_GetGlobalInt(string a) { return 0; }
    public void D_SetGlobalInt(string a, int b) { }
    public void D_PlaySoundAmbient(string a, float b, int c) { }
    public int D_PlaySoundPositional(string a, int b, double c, double d, int e) { return 0; }
    // ==============================================================================================================================
}