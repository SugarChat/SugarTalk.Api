using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Tencent;

public enum CloudRecordingEventType
{
    [Description("云端录制启动")]
    CloudRecordingRecorderStart = 301,

    [Description("云端录制退出")]
    CloudRecordingRecorderStop = 302,

    [Description("录制文件上传启动（对象存储）")]
    CloudRecordingUploadStart = 303,

    [Description("生成并上传 m3u8 文件（首次）")]
    CloudRecordingFileInfo = 304,

    [Description("录制文件上传结束")]
    CloudRecordingUploadStop = 305,

    [Description("录制任务迁移（容灾）")]
    CloudRecordingFailover = 306,

    [Description("生成第一个 ts 切片后回调")]
    CloudRecordingFileSlice = 307,

    [Description("解码图片下载错误")]
    CloudRecordingDownloadImageError = 309,

    [Description("MP4 录制结束（对象存储）")]
    CloudRecordingMp4Stop = 310,

    [Description("点播上传成功（含索引信息）")]
    CloudRecordingVodCommit = 311,

    [Description("点播录制任务结束")]
    CloudRecordingVodStop = 312,

    [Description("页面录制启动")]
    WebRecorderStart = 801,

    [Description("页面录制退出")]
    WebRecorderStop = 802,

    [Description("页面录制状态更新")]
    WebRecorderStatusUpdate = 803,

    [Description("页面录制资源受限（时长或分辨率）")]
    WebRecorderResourceLimit = 804
}