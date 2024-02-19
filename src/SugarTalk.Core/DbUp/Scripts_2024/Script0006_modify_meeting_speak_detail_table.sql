ALTER TABLE `meeting_speak_detail` ADD COLUMN `meeting_id` varchar(36) not null;
ALTER TABLE `meeting_speak_detail` ADD COLUMN `meeting_sub_id` varchar(36) null;
ALTER TABLE `meeting_speak_detail` modify `meeting_record_id` varchar(36) null;

CREATE INDEX idx_meeting_id ON meeting_speak_detail (meeting_id);
CREATE INDEX idx_meeting_sub_id ON meeting_speak_detail (meeting_sub_id);