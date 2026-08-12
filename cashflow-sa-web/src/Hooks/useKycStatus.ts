import { useCallback, useEffect, useState } from "react";
import { useAuth } from "./useAuth";
import { getKycStatus, type KycStatusResponse, type KycStatusView } from "../Services/kycService";

interface UseKycStatusResult {
  status: KycStatusView | null; // null while loading, or when not applicable (non-SME role)
  application: KycStatusResponse | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

// Only meaningful for SME users -- returns status: null (and isLoading:
// false) for anyone else so callers don't need to guard on role first.
export function useKycStatus(): UseKycStatusResult {
  const { user } = useAuth();
  const [application, setApplication] = useState<KycStatusResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshCount, setRefreshCount] = useState(0);

  const refetch = useCallback(() => setRefreshCount((c) => c + 1), []);

  useEffect(() => {
    async function loadStatus() {
      if (user?.role !== "SME" || !user.profileId) {
        setIsLoading(false);
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        const result = await getKycStatus(user.profileId);
        setApplication(result);
      } catch {
        setError("Could not check your FICA verification status.");
      } finally {
        setIsLoading(false);
      }
    }

    loadStatus();
  }, [user?.role, user?.profileId, refreshCount]);

  if (user?.role !== "SME") {
    return { status: null, application: null, isLoading: false, error: null, refetch };
  }

  const status: KycStatusView | null = isLoading ? null : (application?.status ?? "NotSubmitted");

  return { status, application, isLoading, error, refetch };
}
