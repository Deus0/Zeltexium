using UnityEngine;
using System.Collections;

public class TestPianoNote : MonoBehaviour
{

    public float Duration = 1;
    public int KeyPitch = 48;

    public Note Note;
    private int sampleRate;
    private AudioSource source;

    // Use this for initialization
    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Note = new Note(Scale.pitches[KeyPitch], Duration, sampleRate, source);
            Note.Play();
        }
        if (KeyPitch < Scale.pitches.Length && Input.GetKeyDown(KeyCode.H))
        {
            KeyPitch++;
        }
        if (KeyPitch > 0 && Input.GetKeyDown(KeyCode.F))
        {
            KeyPitch--;
        }
    }
}

public class Note
{
    private Pitch pitch;
    private float length;
    private int sampleRate;
    private AudioClip clip;
    private AudioSource source;

    public Note(Pitch Pitch, float Length, int SampleRate, AudioSource Source)
    {
        this.pitch = Pitch;
        this.length = Length;
        this.sampleRate = SampleRate;
        this.source = Source;
        this.clip = AudioClip.Create("Procedural Tone", (int)(sampleRate * length), 1, sampleRate, false);

        int count = 0;
        float[] data = new float[(int)(sampleRate * length)];
        while (count < data.Length)
        {
            data[count] = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * pitch.Frequency * count / sampleRate));
            count++;
        }

        clip.SetData(data, 0);
        source.clip = clip;
    }

    public void Play()
    {
        source.Play();
    }
}

public class Pitch
{
    private string name;
    private float frequency;

    public float Frequency
    {
        get { return frequency; }
    }

    public Pitch(string Name, float Frequency)
    {
        this.name = Name;
        this.frequency = Frequency;
    }

    public override string ToString()
    {
        return name;
    }
}

public static class Scale
{
    public static Pitch[] pitches =
    {
             new Pitch("C0", 16.35f),
             new Pitch("C#0/Db0", 17.32f),
             new Pitch("D0", 18.35f),
             new Pitch("D#0/Eb0", 19.45f),
             new Pitch("E0", 20.6f),
             new Pitch("F0", 21.83f),
             new Pitch("F#0/Gb0", 23.12f),
             new Pitch("G0", 24.5f),
             new Pitch("G#0/Ab0", 25.96f),
             new Pitch("A0", 27.5f),
             new Pitch("A#0/Bb0", 29.14f),
             new Pitch("B0", 30.87f),
             new Pitch("C1", 32.7f),
             new Pitch("C#1/Db1", 34.65f),
             new Pitch("D1", 36.71f),
             new Pitch("D#1/Eb1", 38.89f),
             new Pitch("E1", 41.2f),
             new Pitch("F1", 43.65f),
             new Pitch("F#1/Gb1", 46.25f),
             new Pitch("G1", 49f),
             new Pitch("G#1/Ab1", 51.91f),
             new Pitch("A1", 55f),
             new Pitch("A#1/Bb1", 58.27f),
             new Pitch("B1", 61.74f),
             new Pitch("C2", 65.41f),
             new Pitch("C#2/Db2", 69.3f),
             new Pitch("D2", 73.42f),
             new Pitch("D#2/Eb2", 77.78f),
             new Pitch("E2", 82.41f),
             new Pitch("F2", 87.31f),
             new Pitch("F#2/Gb2", 92.5f),
             new Pitch("G2", 98f),
             new Pitch("G#2/Ab2", 103.83f),
             new Pitch("A2", 110f),
             new Pitch("A#2/Bb2", 116.54f),
             new Pitch("B2", 123.47f),
             new Pitch("C3", 130.81f),
             new Pitch("C#3/Db3", 138.59f),
             new Pitch("D3", 146.83f),
             new Pitch("D#3/Eb3", 155.56f),
             new Pitch("E3", 164.81f),
             new Pitch("F3", 174.61f),
             new Pitch("F#3/Gb3", 185f),
             new Pitch("G3", 196f),
             new Pitch("G#3/Ab3", 207.65f),
             new Pitch("A3", 220f),
             new Pitch("A#3/Bb3", 233.08f),
             new Pitch("B3", 246.94f),
             new Pitch("C4", 261.63f),
             new Pitch("C#4/Db4", 277.18f),
             new Pitch("D4", 293.66f),
             new Pitch("D#4/Eb4", 311.13f),
             new Pitch("E4", 329.63f),
             new Pitch("F4", 349.23f),
             new Pitch("F#4/Gb4", 369.99f),
             new Pitch("G4", 392f),
             new Pitch("G#4/Ab4", 415.3f),
             new Pitch("A4", 440f),
             new Pitch("A#4/Bb4", 466.16f),
             new Pitch("B4", 493.88f),
             new Pitch("C5", 523.25f),
             new Pitch("C#5/Db5", 554.37f),
             new Pitch("D5", 587.33f),
             new Pitch("D#5/Eb5", 622.25f),
             new Pitch("E5", 659.25f),
             new Pitch("F5", 698.46f),
             new Pitch("F#5/Gb5", 739.99f),
             new Pitch("G5", 783.99f),
             new Pitch("G#5/Ab5", 830.61f),
             new Pitch("A5", 880f),
             new Pitch("A#5/Bb5", 932.33f),
             new Pitch("B5", 987.77f),
             new Pitch("C6", 1046.5f),
             new Pitch("C#6/Db6", 1108.73f),
             new Pitch("D6", 1174.66f),
             new Pitch("D#6/Eb6", 1244.51f),
             new Pitch("E6", 1318.51f),
             new Pitch("F6", 1396.91f),
             new Pitch("F#6/Gb6", 1479.98f),
             new Pitch("G6", 1567.98f),
             new Pitch("G#6/Ab6", 1661.22f),
             new Pitch("A6", 1760f),
             new Pitch("A#6/Bb6", 1864.66f),
             new Pitch("B6", 1975.53f),
             new Pitch("C7", 2093f),
             new Pitch("C#7/Db7", 2217.46f),
             new Pitch("D7", 2349.32f),
             new Pitch("D#7/Eb7", 2489.02f),
             new Pitch("E7", 2637.02f),
             new Pitch("F7", 2793.83f),
             new Pitch("F#7/Gb7", 2959.96f),
             new Pitch("G7", 3135.96f),
             new Pitch("G#7/Ab7", 3322.44f),
             new Pitch("A7", 3520f),
             new Pitch("A#7/Bb7", 3729.31f),
             new Pitch("B7", 3951.07f),
             new Pitch("C8", 4186.01f),
             new Pitch("C#8/Db8", 4434.92f),
             new Pitch("D8", 4698.63f),
             new Pitch("D#8/Eb8", 4978.03f),
             new Pitch("E8", 5274.04f),
             new Pitch("F8", 5587.65f),
             new Pitch("F#8/Gb8", 5919.91f),
             new Pitch("G8", 6271.93f),
             new Pitch("G#8/Ab8", 6644.88f),
             new Pitch("A8", 7040f),
             new Pitch("A#8/Bb8", 7458.62f),
             new Pitch("B8", 7902.13f)
         };
}