import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import {
  approveKycApplication,
  getPendingKycApplications,
  rejectKycApplication,
  type PendingKycApplication,
} from "../../Services/kycService";
import Modal from "../../components/Shared/Modal/Modal";
import styles from "./KycReview.module.css";

type RowAction = "approve" | "reject" | null;

export default function KycReview() {
  const { user } = useAuth();

  const [applications, setApplications] = useState<PendingKycApplication[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [rowBusy, setRowBusy] = useState<Record<string, RowAction>>({});

  const [rejectTarget, setRejectTarget] =
    useState<PendingKycApplication | null>(null);

  const [rejectNotes, setRejectNotes] = useState("");
  const [rejectError, setRejectError] = useState<string | null>(null);

  const fetchApplications = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const result = await getPendingKycApplications();
      setApplications(result);
    } catch {
      setError("Could not load pending KYC applications. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchApplications();
  }, [fetchApplications]);

  async function handleApprove(application: PendingKycApplication) {
    if (!user?.userId) {
      setError("Your user session could not be identified.");
      return;
    }

    setError(null);

    setRowBusy((previous) => ({
      ...previous,
      [application.applicationId]: "approve",
    }));

    try {
      await approveKycApplication(
        application.applicationId,
        user.userId
      );

      await fetchApplications();
    } catch {
      setError(
        `Could not approve the KYC application for ${application.companyName}. Please try again.`
      );
    } finally {
      setRowBusy((previous) => ({
        ...previous,
        [application.applicationId]: null,
      }));
    }
  }

  function openRejectModal(application: PendingKycApplication) {
    setRejectTarget(application);
    setRejectNotes("");
    setRejectError(null);
  }

  function closeRejectModal() {
    setRejectTarget(null);
    setRejectNotes("");
    setRejectError(null);
  }

  async function confirmReject() {
    if (!user?.userId) {
      setRejectError("Your user session could not be identified.");
      return;
    }

    if (!rejectTarget) {
      return;
    }

    const notes = rejectNotes.trim();

    if (!notes) {
      setRejectError(
        "A rejection reason is required so the SME knows what needs to be fixed."
      );
      return;
    }

    const applicationId = rejectTarget.applicationId;

    setRejectError(null);

    setRowBusy((previous) => ({
      ...previous,
      [applicationId]: "reject",
    }));

    try {
      await rejectKycApplication(
        applicationId,
        user.userId,
        notes
      );

      closeRejectModal();
      await fetchApplications();
    } catch {
      setRejectError(
        "Could not reject this KYC application. Please try again."
      );
    } finally {
      setRowBusy((previous) => ({
        ...previous,
        [applicationId]: null,
      }));
    }
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Compliance</p>

          <h1>KYC application review</h1>

          <p className={styles.subhead}>
            Review pending KYC applications and approve or reject the
            application as a whole.
          </p>
        </div>

        <button
          type="button"
          className={styles.refreshButton}
          onClick={fetchApplications}
          disabled={isLoading}
        >
          {isLoading ? "Refreshing..." : "Refresh"}
        </button>
      </header>

      <section className={styles.summary}>
        <div className={styles.summaryCard}>
          <span className={styles.summaryLabel}>Pending applications</span>
          <strong className={styles.summaryValue}>
            {applications.length}
          </strong>
        </div>
      </section>

      {error && (
        <div className={styles.error} role="alert">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className={styles.status}>
          <p>Loading pending KYC applications...</p>
        </div>
      ) : applications.length === 0 ? (
        <div className={styles.empty}>
          <h2>No pending KYC applications</h2>
          <p>
            There are currently no KYC applications waiting for
            application-level review.
          </p>
        </div>
      ) : (
        <section className={styles.listSection}>
          <div className={styles.listHeader}>
            <div>
              <h2>Pending applications</h2>
              <p>
                These applications have reached the application-level
                review stage.
              </p>
            </div>

            <span className={styles.countBadge}>
              {applications.length}
            </span>
          </div>

          <div className={styles.list}>
            {applications.map((application) => {
              const busy = rowBusy[application.applicationId];

              return (
                <article
                  key={application.applicationId}
                  className={styles.row}
                >
                  <div className={styles.applicationInfo}>
                    <div className={styles.companyHeader}>
                      <h3>{application.companyName}</h3>

                      <span className={styles.pendingBadge}>
                        Pending
                      </span>
                    </div>

                    <dl className={styles.details}>
                      <div>
                        <dt>Application ID</dt>
                        <dd>{application.applicationId}</dd>
                      </div>

                      <div>
                        <dt>SME ID</dt>
                        <dd>{application.smeId}</dd>
                      </div>

                      <div>
                        <dt>Submitted</dt>
                        <dd>
                          {new Date(
                            application.applicationDate
                          ).toLocaleDateString("en-ZA", {
                            year: "numeric",
                            month: "short",
                            day: "numeric",
                          })}
                        </dd>
                      </div>
                    </dl>
                  </div>

                  <div className={styles.actions}>
                    <button
                      type="button"
                      className={styles.approveButton}
                      disabled={!!busy}
                      onClick={() => handleApprove(application)}
                    >
                      {busy === "approve"
                        ? "Approving..."
                        : "Approve"}
                    </button>

                    <button
                      type="button"
                      className={styles.rejectButton}
                      disabled={!!busy}
                      onClick={() => openRejectModal(application)}
                    >
                      Reject
                    </button>
                  </div>
                </article>
              );
            })}
          </div>
        </section>
      )}

      <Modal
        isOpen={!!rejectTarget}
        onClose={closeRejectModal}
        title={`Reject ${rejectTarget?.companyName ?? "KYC application"}`}
      >
        <div className={styles.modalBody}>
          <p className={styles.modalDescription}>
            Rejecting this application will record the reason against
            the KYC application. Provide a clear reason for the SME.
          </p>

          <label
            className={styles.modalLabel}
            htmlFor="kyc-rejection-notes"
          >
            Reason for rejection
          </label>

          <textarea
            id="kyc-rejection-notes"
            className={styles.modalTextarea}
            rows={5}
            value={rejectNotes}
            onChange={(event) => {
              setRejectNotes(event.target.value);

              if (rejectError) {
                setRejectError(null);
              }
            }}
            placeholder="Explain why the KYC application is being rejected and what needs to be corrected."
            disabled={
              rejectTarget
                ? rowBusy[rejectTarget.applicationId] === "reject"
                : false
            }
          />

          {rejectError && (
            <p className={styles.modalError} role="alert">
              {rejectError}
            </p>
          )}

          <div className={styles.modalActions}>
            <button
              type="button"
              className={styles.cancelButton}
              onClick={closeRejectModal}
              disabled={
                rejectTarget
                  ? rowBusy[rejectTarget.applicationId] === "reject"
                  : false
              }
            >
              Cancel
            </button>

            <button
              type="button"
              className={styles.rejectButton}
              onClick={confirmReject}
              disabled={
                !rejectTarget ||
                rowBusy[rejectTarget.applicationId] === "reject"
              }
            >
              {rejectTarget &&
              rowBusy[rejectTarget.applicationId] === "reject"
                ? "Rejecting..."
                : "Confirm rejection"}
            </button>
          </div>
        </div>
      </Modal>
    </main>
  );
}