import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr";

import { API_BASE_URL } from "../utils/env";
import { getAccessToken } from "../utils/storage";
import type { NotificationItem } from "./notificationService";

export interface FundingUpdate {
  campaignId: string;
  invoiceId: string;
  targetAmount: number;
  fundedAmount: number;
  status: string;
}

type NotificationListener = (
  notification: NotificationItem,
) => void;

type FundingUpdateListener = (
  update: FundingUpdate,
) => void;

let connection: HubConnection | null = null;
let startPromise: Promise<HubConnection> | null = null;

const notificationListeners = new Set<NotificationListener>();
const fundingUpdateListeners = new Set<FundingUpdateListener>();

/**
 * Creates the SignalR connection and registers the shared
 * notification/funding listeners.
 */
function createConnection(): HubConnection {
  const hub = new HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/notifications`, {
      accessTokenFactory: () => getAccessToken() ?? "",
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(LogLevel.Warning)
    .build();

  /*
   * Existing notification event.
   *
   * Anything subscribed through subscribeToNotifications()
   * receives this event.
   */
  hub.on(
    "notificationReceived",
    (notification: NotificationItem) => {
      notificationListeners.forEach((listener) => {
        try {
          listener(notification);
        } catch (error) {
          console.error(
            "[SignalR] Notification listener failed:",
            error,
          );
        }
      });
    },
  );

  /*
   * New funding event.
   *
   * Backend sends:
   *
   * {
   *   campaignId,
   *   invoiceId,
   *   targetAmount,
   *   fundedAmount,
   *   status
   * }
   */
  hub.on(
    "fundingUpdated",
    (update: FundingUpdate) => {
      console.log(
        "[SignalR] Funding update received:",
        update,
      );

      fundingUpdateListeners.forEach((listener) => {
        try {
          listener(update);
        } catch (error) {
          console.error(
            "[SignalR] Funding listener failed:",
            error,
          );
        }
      });
    },
  );

  /*
   * If SignalR reconnects, the same event handlers remain
   * attached to this HubConnection instance.
   */
  hub.onreconnecting((error) => {
    console.warn(
      "[SignalR] Notification hub reconnecting...",
      error,
    );
  });

  hub.onreconnected((connectionId) => {
    console.log(
      "[SignalR] Notification hub reconnected:",
      connectionId,
    );
  });

  hub.onclose((error) => {
    if (error) {
      console.warn(
        "[SignalR] Notification hub closed:",
        error,
      );
    } else {
      console.log(
        "[SignalR] Notification hub closed.",
      );
    }

    if (connection === hub) {
      connection = null;
    }
  });

  return hub;
}

/**
 * Ensures that the shared SignalR connection is running.
 */
async function ensureConnection(): Promise<HubConnection> {
  if (
    connection &&
    connection.state === HubConnectionState.Connected
  ) {
    return connection;
  }

  if (startPromise) {
    return startPromise;
  }

  const hub = connection ?? createConnection();

  connection = hub;

  startPromise = (async () => {
    if (
      hub.state !== HubConnectionState.Connected &&
      hub.state !== HubConnectionState.Connecting &&
      hub.state !== HubConnectionState.Reconnecting
    ) {
      await hub.start();
    }

    return hub;
  })();

  try {
    return await startPromise;
  } catch (error) {
    if (connection === hub) {
      connection = null;
    }

    throw error;
  } finally {
    startPromise = null;
  }
}

/**
 * Existing API used by the Notifications page.
 *
 * This is kept for compatibility with your existing code.
 */
export async function startNotificationHub(
  onNotification?: NotificationListener,
): Promise<HubConnection> {
  if (onNotification) {
    notificationListeners.add(onNotification);
  }

  try {
    return await ensureConnection();
  } catch (error) {
    if (onNotification) {
      notificationListeners.delete(onNotification);
    }

    throw error;
  }
}

/**
 * Subscribe to normal application notifications.
 *
 * Returns an unsubscribe function.
 */
export async function subscribeToNotifications(
  onNotification: NotificationListener,
): Promise<() => void> {
  notificationListeners.add(onNotification);

  try {
    await ensureConnection();
  } catch (error) {
    notificationListeners.delete(onNotification);
    throw error;
  }

  return () => {
    notificationListeners.delete(onNotification);
  };
}

/**
 * Subscribe to invoice/campaign funding updates.
 *
 * Returns an unsubscribe function.
 *
 * Used by:
 * - Investor Marketplace
 * - Investor Dashboard
 * - SME Dashboard
 * - SME Invoices
 * - Admin funding views
 */
export async function subscribeToFundingUpdates(
  onFundingUpdate: FundingUpdateListener,
): Promise<() => void> {
  fundingUpdateListeners.add(onFundingUpdate);

  try {
    await ensureConnection();
  } catch (error) {
    fundingUpdateListeners.delete(onFundingUpdate);
    throw error;
  }

  return () => {
    fundingUpdateListeners.delete(onFundingUpdate);
  };
}

/**
 * Returns the current SignalR connection, if one exists.
 */
export function getNotificationHubConnection(): HubConnection | null {
  return connection;
}

/**
 * Stops the shared SignalR connection and clears listeners.
 */
export async function stopNotificationHub(): Promise<void> {
  if (!connection) {
    notificationListeners.clear();
    fundingUpdateListeners.clear();
    return;
  }

  const current = connection;

  connection = null;
  startPromise = null;

  notificationListeners.clear();
  fundingUpdateListeners.clear();

  if (
    current.state !== HubConnectionState.Disconnected
  ) {
    await current.stop();
  }
}