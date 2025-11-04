CREATE TABLE `meeting_invitation_record` (
  `id` INT PRIMARY KEY AUTO_INCREMENT,
  `meeting_id` VARCHAR(36) NOT NULL,
  `meeting_sub_id` VARCHAR(36) NULL,
  `user_id` INT NOT NULL,
  `user_name` VARCHAR(255) NOT NULL,
  `inviter_id` INT NOT NULL,
  `invitation_status` INT NULL,
  `created_date` DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3)
)charset=utf8mb4;