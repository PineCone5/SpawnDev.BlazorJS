﻿using Microsoft.JSInterop;

namespace SpawnDev.BlazorJS.JSObjects
{
    /// <summary>
    /// The MediaStreamTrack interface of the Media Capture and Streams API represents a single media track within a stream; typically, these are audio or video tracks, but other track types may exist as well.<br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/MediaStreamTrack
    /// </summary>
    public class MediaStreamTrack : EventTarget
    {
        /// <summary>
        /// A Boolean whose value of true if the track is enabled, that is allowed to render the media source stream; or false if it is disabled, that is not rendering the media source stream but silence and blackness. If the track has been disconnected, this value can be changed but has no more effect.<br/>
        /// Note: You can implement standard "mute" functionality by setting enabled to false. The muted property refers to a condition in which there's no media because of a technical issue.
        /// </summary>
        public bool Enabled { get => JSRef!.Get<bool>("enabled"); set => JSRef!.Set("enabled", value); }
        /// <summary>
        /// Returns a Boolean value indicating whether the track is unable to provide media data due to a technical issue.<br/>
        /// Note: You can implement standard "mute" functionality by setting enabled to false, and unmute the media by setting it back to true again.
        /// </summary>
        public bool Muted => JSRef!.Get<bool>("muted");
        /// <summary>
        /// Returns a string containing a unique identifier (GUID) for the track; it is generated by the browser.
        /// </summary>
        public string Id => JSRef!.Get<string>("id");
        /// <summary>
        /// Returns a string set to "audio" if the track is an audio track and to "video", if it is a video track. It doesn't change if the track is disassociated from its source.
        /// </summary>
        public string Kind => JSRef!.Get<string>("kind");
        /// <summary>
        /// A string that may be used by the web application to provide a hint as to what type of content the track contains to guide how it should be treated by API consumers.
        /// </summary>
        public string ContentHint => JSRef!.Get<string>("contentHint");
        /// <summary>
        /// Returns a string containing a user agent-assigned label that identifies the track source, as in "internal microphone". The string may be left empty and is empty as long as no source has been connected. When the track is disassociated from its source, the label is not changed.
        /// </summary>
        public string Label => JSRef!.Get<string>("label");
        /// <summary>
        /// Returns an enumerated string giving the status of the track. This will be one of the following values:<br/>
        /// "live" which indicates that an input is connected and does its best-effort in providing real-time data. In that case, the output of data can be switched on or off using the enabled attribute.<br/>
        /// "ended" which indicates that the input is not giving any more data and will never provide new data.
        /// </summary>
        public string ReadyState => JSRef!.Get<string>("readyState");
        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="_ref"></param>
        public MediaStreamTrack(IJSInProcessObjectReference _ref) : base(_ref) { }
        /// <summary>
        /// Returns a MediaTrackSettings object containing the current values of each of the MediaStreamTrack's constrainable properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSettings<T>() where T : MediaTrackSettings => JSRef!.Call<T>("getSettings");
        /// <summary>
        /// Returns a MediaTrackSettings object containing the current values of each of the MediaStreamTrack's constrainable properties.
        /// </summary>
        /// <returns></returns>
        public MediaTrackSettings GetSettings() => JSRef!.Call<MediaTrackSettings>("getSettings");
        /// <summary>
        /// Stops playing the source associated to the track, both the source and the track are disassociated. The track state is set to ended.
        /// </summary>
        public void Stop() => JSRef!.CallVoid("stop");
        /// <summary>
        /// The clone() method of the MediaStreamTrack interface creates a duplicate of the MediaStreamTrack. This new MediaStreamTrack object is identical except for its unique id.
        /// </summary>
        /// <returns>A new MediaStreamTrack instance which is identical to the one clone() was called, except for its new unique id.</returns>
        public MediaStreamTrack Clone() => JSRef!.Call<MediaStreamTrack>("clone");
        /// <summary>
        /// The applyConstraints() method of the MediaStreamTrack interface applies a set of constraints to the track; these constraints let the website or app establish ideal values and acceptable ranges of values for the constrainable properties of the track, such as frame rate, dimensions, echo cancellation, and so forth.
        /// </summary>
        /// <param name="constraints">A MediaTrackConstraints object listing the constraints to apply to the track's constrainable properties; any existing constraints are replaced with the new values specified, and any constrainable properties not included are restored to their default constraints. If this parameter is omitted, all currently set custom constraints are cleared. This object represents the basic set of constraints that must apply for the Promise to resolve. The object may contain an advanced property containing an array of additional MediaTrackConstraints objects, which are treated as exact requires.</param>
        /// <returns>A Promise which resolves when the constraints have been successfully applied. If the constraints cannot be applied, the promise is rejected with a OverconstrainedError that is a DOMException whose name is OverconstrainedError with additional parameters, and, to indicate that the constraints could not be met. This can happen if the specified constraints are too strict to find a match when attempting to configure the track.</returns>
        public Task ApplyConstraints(object constraints) => JSRef!.CallVoidAsync("applyConstraints", constraints);
        /// <summary>
        /// The applyConstraints() method of the MediaStreamTrack interface applies a set of constraints to the track; these constraints let the website or app establish ideal values and acceptable ranges of values for the constrainable properties of the track, such as frame rate, dimensions, echo cancellation, and so forth.
        /// </summary>
        /// <param name="constraints">A MediaTrackConstraints object listing the constraints to apply to the track's constrainable properties; any existing constraints are replaced with the new values specified, and any constrainable properties not included are restored to their default constraints. If this parameter is omitted, all currently set custom constraints are cleared. This object represents the basic set of constraints that must apply for the Promise to resolve. The object may contain an advanced property containing an array of additional MediaTrackConstraints objects, which are treated as exact requires.</param>
        /// <returns>A Promise which resolves when the constraints have been successfully applied. If the constraints cannot be applied, the promise is rejected with a OverconstrainedError that is a DOMException whose name is OverconstrainedError with additional parameters, and, to indicate that the constraints could not be met. This can happen if the specified constraints are too strict to find a match when attempting to configure the track.</returns>
        public Task ApplyConstraints(MediaTrackConstraints constraints) => JSRef!.CallVoidAsync("applyConstraints", constraints);
        /// <summary>
        /// The applyConstraints() method of the MediaStreamTrack interface applies a set of constraints to the track; these constraints let the website or app establish ideal values and acceptable ranges of values for the constrainable properties of the track, such as frame rate, dimensions, echo cancellation, and so forth.
        /// </summary>
        /// <returns></returns>
        public Task ApplyConstraints() => JSRef!.CallVoidAsync("applyConstraints");
        /// <summary>
        /// The getConstraints() method of the MediaStreamTrack interface returns a MediaTrackConstraints object containing the set of constraints most recently established for the track using a prior call to applyConstraints(). These constraints indicate values and ranges of values that the website or application has specified are required or acceptable for the included constrainable properties.
        /// </summary>
        /// <returns></returns>
        public MediaTrackConstraints GetConstraints() => JSRef!.Call<MediaTrackConstraints>("getConstraints");
        /// <summary>
        /// Returns the a list of constrainable properties available for the MediaStreamTrack.
        /// </summary>
        /// <returns></returns>
        public MediaTrackCapabilities GetCapabilities() => JSRef!.Call<MediaTrackCapabilities>("getCapabilities");
        /// <summary>
        /// The ended event of the MediaStreamTrack interface is fired when playback or streaming has stopped because the end of the media was reached or because no further data is available.
        /// </summary>
        public JSEventCallback<Event> OnEnded { get => new JSEventCallback<Event>("ended", AddEventListener, RemoveEventListener); set { } }
        /// <summary>
        /// The mute event is sent to a MediaStreamTrack when the track's source is temporarily unable to provide media data.
        /// </summary>
        public JSEventCallback<Event> OnMute { get => new JSEventCallback<Event>("mute", AddEventListener, RemoveEventListener); set { } }
        /// <summary>
        /// The unmute event is sent to a MediaStreamTrack when the track's source is once again able to provide media data after a period of not being able to do so.
        /// </summary>
        public JSEventCallback<Event> OnUnMute { get => new JSEventCallback<Event>("unmute", AddEventListener, RemoveEventListener); set { } }
    }
}
