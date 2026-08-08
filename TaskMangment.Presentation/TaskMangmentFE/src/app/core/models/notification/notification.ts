export interface Notification {
  id: number;
  title: string;
  message: string;
  isRead: boolean;
}

export interface TaskNotificationAttachment {
  id: number;
  fileName: string;
  url: string;
  urlDownload?: string;
  contentType?: string;
  size?: number;
}

export interface TaskNotification {
  id: number;
  message: string;
  isRead: boolean;
  notificationType?: string | number;
  createdDate: string;
  taskId?: number;
  referenceId?: number;
  attachments?: TaskNotificationAttachment[];
}
