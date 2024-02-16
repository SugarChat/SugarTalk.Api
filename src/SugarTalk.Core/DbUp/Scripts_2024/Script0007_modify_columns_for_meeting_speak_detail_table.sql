ALTER TABLE `meeting_speak_detail` DROP INDEX idx_meeting_id;
ALTER TABLE `meeting_speak_detail` DROP INDEX idx_meeting_sub_id;

ALTER TABLE `meeting_speak_detail` DROP COLUMN `meeting_id`;
ALTER TABLE `meeting_speak_detail` DROP COLUMN `meeting_sub_id`;

ALTER TABLE `meeting_speak_detail` ADD COLUMN `track_id` varchar(128) not null;
ALTER TABLE `meeting_speak_detail` ADD COLUMN `egress_id` varchar(128) not null;
ALTER TABLE `meeting_speak_detail` ADD COLUMN `meeting_number` varchar(48) not null;
ALTER TABLE `meeting_speak_detail` ADD COLUMN `file_path` varchar(256) not null;

ALTER TABLE `meeting_speak_detail` modify `meeting_record_id` varchar(36) not null;

CREATE INDEX idx_track_id ON meeting_speak_detail (track_id);
CREATE INDEX idx_egress_id ON meeting_speak_detail (egress_id);
CREATE INDEX idx_meeting_number ON meeting_speak_detail (meeting_number);