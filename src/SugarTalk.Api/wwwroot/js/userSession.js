function UserSession(user, isSelf) {
    this.id = user.id;
    this.userName = user.userName;
    this.isSelf = isSelf;
    this.rtcPeer = this.isSelf ? this.createPeerSendonly() : this.createPeerRecvonly();
};
UserSession.prototype.getPlayer = function () {
    this.getPlayer = document.getElementById(this.id);
    if (!this.player) {
        this.player = document.createElement("video");
        this.player.id = this.id;
        this.player.autoplay = true;
        document.body.appendChild(this.player);
    }
    return this.player;
};
UserSession.prototype.createPeerSendonly = function () {
    const rtcPeer = new RTCPeerConnection();
    rtcPeer.addEventListener("icecandidate", candidate => server.invoke("ProcessCandidateAsync", this.id, candidate));
    navigator.mediaDevices.getUserMedia({ video: true, audio: true }).then(localStream => {
        this.getPlayer().srcObject = localStream;
        localStream.getTracks().forEach(track => rtcPeer.addTrack(track, localStream));
        rtcPeer.createOffer({ offerToReceiveAudio: 0, offerToReceiveVideo: 0 }).then(offer => {
            rtcPeer.setLocalDescription(offer);
            server.invoke("ProcessOfferAsync", this.id, offer.sdp);
        });
    });
    return rtcPeer;
};
UserSession.prototype.createPeerRecvonly = function () {
    const rtcPeer = new RTCPeerConnection();
    rtcPeer.addEventListener("addstream", e => { this.getPlayer().srcObject = e.stream; });
    rtcPeer.addEventListener("icecandidate", candidate => server.invoke("ProcessCandidateAsync", this.id, candidate));
    rtcPeer.createOffer({ offerToReceiveAudio: 1, offerToReceiveVideo: 1 }).then(offer => {
        rtcPeer.setLocalDescription(offer);
        server.invoke("ProcessOfferAsync", this.id, offer.sdp);
    });
    return rtcPeer;
};
UserSession.prototype.processAnswer = function (answerSDP) {
    this.rtcPeer.setRemoteDescription(new RTCSessionDescription({ type: "answer", sdp: answerSDP }));
};
UserSession.prototype.addCandidate = function (candidate) {
    this.rtcPeer.addIceCandidate(candidate);
};
UserSession.prototype.dispose = function () {
    this.rtcPeer.close();
    document.body.removeChild(this.player);
};