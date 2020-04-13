using BarRaider.SdTools;
using RtMidi.Core;
using RtMidi.Core.Devices.Infos;
using RtMidi.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceMeeter.Midi
{
    static class MidiCommandHandler
    {
        private const string MIDI_COMMAND_PREFIX = "sendmidi(";
        private const int MIDI_COMMAND_PARAMS_NUM = 5;

        public static void HandleMidiParameters(string parameters)
        {
            int searchStart = 0;
            if (String.IsNullOrEmpty(parameters))
            {
                return;
            }

            parameters = parameters.ToLowerInvariant();
            if (parameters.Contains(MIDI_COMMAND_PREFIX))
            {
                int position = parameters.IndexOf(MIDI_COMMAND_PREFIX, searchStart);
                while (position >= 0)
                {
                    // Extract the midi command from the entire command string
                    int midiCommandEnd = parameters.IndexOf(')', position);
                    if (midiCommandEnd < 0)
                    {
                        Logger.Instance.LogMessage(TracingLevel.WARN, $"Invalid Midi Command in string: {parameters}");
                        return;
                    }

                    // Get all the parameters inside the quotes
                    int paramsStart = position + MIDI_COMMAND_PREFIX.Length;
                    string midiCommand = parameters.Substring(paramsStart, midiCommandEnd - paramsStart);

                    // Split it out to the various params
                    string[] commands = midiCommand.Split(',');
                    if (commands.Length != MIDI_COMMAND_PARAMS_NUM)
                    {
                        Logger.Instance.LogMessage(TracingLevel.WARN, $"Invalid Number of Midi params in command: {midiCommand}");
                        return;
                    }

                    // Trim the commands
                    for (int idx = 0; idx < commands.Length; idx++)
                    {
                        commands[idx] = commands[idx].Trim();
                    }

                    MidiCommandHandler.HandleMidiRequest(commands[0], commands[1], commands[2], commands[3], commands[4]);
                    searchStart = midiCommandEnd;

                    // Continue the while loop until all midi commands have been dealt with
                    position = parameters.IndexOf(MIDI_COMMAND_PREFIX, searchStart);
                }
            }
        }

        public static void HandleMidiRequest(string deviceName, string commandType, string channel, string key, string value)
        {
            if (!Int32.TryParse(channel, out int channelId))
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"HandleMidiRequest called but invalid value for channel {channel}");
                // TODO: Write an error log
                return;
            }

            if (channelId < 0 || channelId > 15)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"HandleMidiRequest called but invalid value for channelId {channelId}");
                return;
            }

            // We need to reduce one from channelId because channel1 is actually a value of 0
            channelId--;

            if (!Int32.TryParse(key, out int keyId))
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"HandleMidiRequest called but invalid value for key {key}");
                return;
            }

            if (keyId < 0 || keyId > 127)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"HandleMidiRequest called but invalid value for keyId {keyId}");
                return;
            }

            if (!Int32.TryParse(value, out int valueId))
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"HandleMidiRequest called but invalid value for value {value}");
                return;
            }

            if (valueId < 0 || valueId > 127)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"HandleMidiRequest called but invalid value for valueId {valueId}");
                return;
            }

            var device = GetOutputDevice(deviceName);

            if (device == null)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"HandleMidiRequest called but could not determine output device named: {deviceName}");
                return;
            }
            var outputDevice = device.CreateDevice();
            outputDevice.Open();

            Logger.Instance.LogMessage(TracingLevel.INFO, $"HandleMidiRequest called for {commandType} with values Channel: {channelId} Key: {keyId} Value: {valueId}");
            // Create the correct command to send the midi device
            switch (commandType.ToLowerInvariant())
            {
                case ("note-on"):
                    outputDevice.Send(new RtMidi.Core.Messages.NoteOnMessage((Channel)channelId, (Key)keyId, valueId));
                    break;
                case ("note-off"):
                    outputDevice.Send(new RtMidi.Core.Messages.NoteOffMessage((Channel)channelId, (Key)keyId, valueId));
                    break;
                case ("ctrl-change"):
                    outputDevice.Send(new RtMidi.Core.Messages.ControlChangeMessage((Channel)channelId, keyId, valueId));
                    break;
                default:
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"HandleMidiRequest called but invalid command: {commandType}");
                    break;
            }
        }

        private static IMidiOutputDeviceInfo GetOutputDevice(string deviceName)
        {
            if (String.IsNullOrEmpty(deviceName))
            {
                return null;
            }

            deviceName = deviceName.ToLowerInvariant();
            return MidiDeviceManager.Default.OutputDevices.Where(d => d.Name.ToLowerInvariant().Contains(deviceName)).FirstOrDefault();
        }
    }
}
