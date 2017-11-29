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

        // Define Variables

        // Declare Points

        // Make The Points

        // END OF DECLARATIONS *********************************************//

        // NPC CODE ++++++++++++++++++++++++++++++++++++++++++++++++++++++++//

        nFakeShockTriggered = D_GetGlobalInt("Global_nFakeShockTriggered");

        if (nFakeShockTriggered == 0)
        {
            // Stops the fake sound being played again
            D_SetGlobalInt("Global_nFakeShockTriggered", 1);

            D_PlaySoundAmbient("Sound\SFX\06002.wav", 0.60, 1);
        }
    }
    // END OF NPC CODE +++++++++++++++++++++++++++++++++++++++++++++++++//
    public void Print(string a) { }
    public int D_GetGlobalInt(string a) { return 0; }
    public void D_SetGlobalInt(string a, int b) { }
    public void D_PlaySoundAmbient(string a, float b, int c) { }

    // ==============================================================================================================================
}