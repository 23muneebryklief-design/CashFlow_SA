import { HubConnectionBuilder, LogLevel, type HubConnection } from "@microsoft/signalr";
import { API_BASE_URL } from "../utils/env";
import { getAccessToken } from "../utils/storage";
import type { NotificationItem } from "./notificationService";

let connection: HubConnection | null = null;
let startPromise: Promise<HubConnection> | null = null;

export async function startNotificationHub(
  onNotification: (notification: NotificationItem) => void,
): Promise<HubConnection> {
  if (connection?.state === "Connected") return connection;
  if (startPromise) return startPromise;

  const hub = new HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/notifications`, {
      accessTokenFactory: () => getAccessToken() ?? "",
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(LogLevel.Warning)
    .build();

  hub.on("notificationReceived", onNotification);
  connection = hub;
  startPromise = hub.start().then(() => hub).finally(() => { startPromise = null; });

  try {
    return await startPromise;
  } catch (error) {
    if (connection === hub) connection = null;
    throw error;
  }
}

export async function stopNotificationHub(): Promise<void> {
  if (!connection) return;
  const current = connection;
  connection = null;
  startPromise = null;
  await current.stop();
}
