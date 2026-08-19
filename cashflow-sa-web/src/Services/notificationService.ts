import { api } from "./api";

export interface NotificationItem {
  notificationId: string;
  event: number | string;
  channel: number | string;
  title: string;
  message: string;
  isRead: boolean;
  readAt: string | null;
  createdAt: string;
}

export async function getNotifications(userId: string): Promise<NotificationItem[]> {
  const response = await api.get<NotificationItem[]>(`/Notification/${userId}`);
  return response.data;
}
