using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleTTSClient
{
    public static void SpeakText(string text)
    {
        var asyncResponse = GoogleTTSClient.Synthesize(text, false);
        asyncResponse.completed += (asyncOperation =>
        {
            var webRequest = ((UnityWebRequestAsyncOperation) asyncOperation).webRequest;

            if (webRequest.isHttpError)
            {
                Debug.Log("Error calling Google TTS API Endpoint.");
            }
            else
            {
                var responseText = webRequest.downloadHandler.text;
                var tempFile = Path.Combine(Application.temporaryCachePath, Path.GetRandomFileName() + ".mp3");
                var response = JsonUtility.FromJson<GoogleTTSClient.Response>(responseText);
                var soundBytes = Convert.FromBase64String(response.audioContent);
                
                File.WriteAllBytes(tempFile, soundBytes);
                Game.game.audioControl.Play(tempFile);
            }
        });
    }
    
    public static UnityWebRequestAsyncOperation Synthesize(string input, bool isSSML)
    {
        var game = Game.Get();
        var apiKey = game.googleTtsApiKey;
        var ttsApiEndpoint = "https://texttospeech.googleapis.com/v1beta1/text:synthesize";
        var url = ttsApiEndpoint + "?key=" + apiKey;
        var request = new Request();

        if (isSSML)
        {
            request.input.ssml = input;
        }
        else
        {
            request.input.text = input;
        }
        request.audioConfig.pitch = -6;
        request.voice.name = game.googleTtsVoice;
        request.voice.languageCode = parseLangCodeFromVoiceName(request.voice.name);

        var payload = serializeToJson(request);
        var unityWebRequest = createPost(url, payload);
        var unityAsyncWebResponse = unityWebRequest.SendWebRequest();

        return unityAsyncWebResponse;
    }

    private static string serializeToJson<T>(T obj)
    {
        DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
        MemoryStream msObj = new MemoryStream();  
        js.WriteObject(msObj, obj);  
        msObj.Position = 0;  
        StreamReader sr = new StreamReader(msObj);

        return sr.ReadToEnd();
    }

    private static string parseLangCodeFromVoiceName(string voiceName)
    {
        var parts = voiceName.Split('-');
        if (parts.Length < 3 || parts[0].Length < 2 || parts[0].Length > 3 || parts[1].Length < 2 || parts[1].Length > 3)
        {
            Debug.Log("Cannot parse language code from voice name: " + voiceName);
            return "en-US";
        }
        return string.Join("-", parts[0], parts[1]);
    }

    private static UnityWebRequest createPost(string url, string jsonPayload)
    {
        var r = new UnityWebRequest(url);
        r.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
        r.downloadHandler = new DownloadHandlerBuffer();
        r.method = UnityWebRequest.kHttpVerbPOST;
        r.SetRequestHeader("Accept", "application/json");
        r.SetRequestHeader("Content-Type", "application/json");

        return r;
    }

    [Serializable]
    private class Request
    {
        public SynthesisInput input = new SynthesisInput();
        public VoiceSelectionParams voice = new VoiceSelectionParams();
        public AudioConfig audioConfig = new AudioConfig();
        public TimepointType[] enableTimePointing;
    }

    /**
     * Contains text input to be synthesized. Either text or ssml must be supplied.
     * Supplying both or neither returns google.rpc.Code.INVALID_ARGUMENT.
     * The input size is limited to 5000 characters.
     */
    [Serializable]
    private class SynthesisInput
    {
        [CanBeNull] public string text;
        [CanBeNull] public string ssml;
    }

    [Serializable]
    private class VoiceSelectionParams
    {
        /* Required. The language (and potentially also the region) of the voice expressed as a BCP-47 language tag,
         e.g. "en-US". This should not include a script tag (e.g. use "cmn-cn" rather than "cmn-Hant-cn"), 
         because the script will be inferred from the input provided in the SynthesisInput. The TTS service will use 
         this parameter to help choose an appropriate voice. Note that the TTS service may choose a voice with a 
         slightly different language code than the one selected; it may substitute a different region (e.g. using en-US 
         rather than en-CA if there isn't a Canadian voice available), or even a different language, e.g. using "nb" 
         (Norwegian Bokmal) instead of "no" (Norwegian)". */
        public string languageCode = "en-US";
        
        /* The name of the voice. If not set, the service will choose a voice based on the other parameters
         such as languageCode and gender. */
        [CanBeNull] public string name;
        
        /* The preferred gender of the voice. If not set, the service will choose a voice based on the other parameters
         such as languageCode and name. Note that this is only a preference, not requirement; if a voice of the 
         appropriate gender is not available, the synthesizer should substitute a voice with a different gender 
         rather than failing the request. */
        public SsmlVoiceGender? ssmlGender;
    }

    [Serializable]
    public enum SsmlVoiceGender
    {
        SSML_VOICE_GENDER_UNSPECIFIED,
        MALE,
        FEMALE,
        NEUTRAL
    }

    [Serializable]
    public class AudioConfig
    {
        public AudioEncoding audioEncoding = AudioEncoding.MP3;

        /* Optional. Input only. Speaking rate/speed, in the range [0.25, 4.0]. 1.0 is the
        normal native speed supported by the specific voice. 2.0 is twice as fast,
        and 0.5 is half as fast. If unset(0.0), defaults to the native 1.0 speed.
        Any other values < 0.25 or > 4.0 will return an error. */
        public float? speakingRate;

        /* Optional. Input only. Speaking pitch, in the range [-20.0, 20.0]. 20 means increase 20 semitones from the
         original pitch. -20 means decrease 20 semitones from the original pitch. */
        public float? pitch;

        /* Optional. Input only. Volume gain (in dB) of the normal native volume supported by the specific voice, in
         the range [-96.0, 16.0]. If unset, or set to a value of 0.0 (dB), will play at normal native signal amplitude. 
         A value of -6.0 (dB) will play at approximately half the amplitude of the normal native signal amplitude. 
         A value of +6.0 (dB) will play at approximately twice the amplitude of the normal native signal amplitude. 
         Strongly recommend not to exceed +10 (dB) as there's usually no effective increase in loudness for any value 
         greater than that. */
        public float? volumeGainDb;

        /* Optional. The synthesis sample rate (in hertz) for this audio. When this is specified in SynthesizeSpeechRequest,
         if this is different from the voice's natural sample rate, then the synthesizer will honor this request by 
         converting to the desired sample rate (which might result in worse audio quality), unless the specified sample 
         rate is not supported for the encoding chosen, in which case it will fail the request and return 
         google.rpc.Code.INVALID_ARGUMENT. */
        public float? sampleRateHertz;

        /* Optional. Input only. An identifier which selects 'audio effects' profiles that are applied on (post synthesized)
         text to speech. Effects are applied on top of each other in the order they are given. See audio profiles for 
         current supported profile ids. */
        public string[] effectsProfileId;
    }

    [Serializable]
    public enum AudioEncoding
    {
        AUDIO_ENCODING_UNSPECIFIED,
        LINEAR16,
        MP3,
        MP3_64_KBPS,
        OGG_OPUS,
        MULAW
    }

    [Serializable]
    private enum TimepointType
    {
        TIMEPOINT_TYPE_UNSPECIFIED,
        SSML_MARK
    }

    [Serializable]
    public class Response
    {
        /* The audio data bytes encoded as specified in the request, including the header for encodings that are wrapped
         in containers (e.g. MP3, OGG_OPUS). For LINEAR16 audio, we include the WAV header. Note: as with all bytes 
         fields, protobuffers use a pure binary representation, whereas JSON representations use base64.
         A base64-encoded string. */
        public string audioContent;

        public Timepoint[] timepoints;
        public AudioConfig audioConfig;
    }

    [Serializable]
    /**
     * This contains a mapping between a certain point in the input text and a corresponding time in the output audio.
     */
    public class Timepoint
    {
        public string markName; //Timepoint name as received from the client within tag.
        public float timeSeconds; //Time offset in seconds from the start of the synthesized audio.
    }
}