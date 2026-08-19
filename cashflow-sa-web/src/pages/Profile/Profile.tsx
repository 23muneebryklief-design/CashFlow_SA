import { useNavigate } from "react-router-dom";
import { useAuth } from "../../Hooks/useAuth";
import { useKycStatus } from "../../Hooks/useKycStatus";
import type { KycDocumentStatus, DocumentType } from "../../Services/kycService";
import styles from "./Profile.module.css";

const labels: Record<DocumentType, string> = {
  CompanyRegistration: "Company registration document",
  IdentityDocument: "Director / owner ID document",
  ProofOfAddress: "Proof of business address",
  TaxCertificate: "Tax clearance certificate",
  BankStatement: "Bank statement",
  Other: "Other document",
};

function statusClass(status: KycDocumentStatus["status"]) {
  return styles[`status${status}` as keyof typeof styles] ?? "";
}

export default function Profile() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const { status, application, isLoading, error } = useKycStatus();

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Account</p>
        <h1>Profile</h1>
        <p className={styles.subhead}>Your account details and verification information.</p>
      </header>

      <section className={styles.card}>
        <div className={styles.cardHeader}>
          <div>
            <p className={styles.cardEyebrow}>Account details</p>
            <h2>{user?.email ?? "—"}</h2>
          </div>
          <span className={styles.role}>{user?.role ?? "—"}</span>
        </div>
      </section>

      <section className={styles.card}>
        <div className={styles.cardHeader}>
          <div>
            <p className={styles.cardEyebrow}>Account information</p>
            <h2>Account & security</h2>
          </div>
        </div>
        <div className={styles.infoGrid}>
          <InfoItem label="User ID" value={user?.userId ?? "—"} mono />
          <InfoItem label="Profile ID" value={user?.profileId ?? "Not applicable"} mono />
          <InfoItem label="Email" value={user?.email ?? "—"} />
          <InfoItem label="Role" value={user?.role ?? "—"} />
        </div>
        <div className={styles.securityNote}>
          <strong>Profile editing</strong>
          <span>Your account details are currently managed by the platform. The backend does not yet expose a secure profile-update or password-change endpoint, so no changes are simulated in this page.</span>
        </div>
        <button
          type="button"
          className={styles.secondaryBtn}
          onClick={() => {
            logout();
            navigate("/login", { replace: true });
          }}
        >
          Sign out
        </button>
      </section>

      {user?.role === "SME" && (
        <section className={styles.card}>
          <div className={styles.cardHeader}>
            <div>
              <p className={styles.cardEyebrow}>FICA verification</p>
              <h2>
                {status === "Verified"
                  ? "Verified"
                  : status === "Pending"
                    ? "Under review"
                    : status === "Rejected"
                      ? "Action required"
                      : "Not submitted"}
              </h2>
            </div>
            {status === "Verified" && <span className={`${styles.statusPill} ${styles.statusApproved}`}>Verified</span>}
          </div>

          {isLoading && <p className={styles.muted}>Loading verification details…</p>}
          {error && <p className={styles.error}>{error}</p>}

          {!isLoading && !error && status === "Verified" && (
            <>
              <p className={styles.muted}>
                Your FICA verification is complete. These documents are shown for reference and are read-only.
              </p>
              <DocumentList documents={application?.documents ?? []} />
              <button type="button" className={styles.secondaryBtn} onClick={() => navigate("/fica-verification")}>
                Update documents
              </button>
            </>
          )}

          {!isLoading && !error && status === "Pending" && (
            <>
              <p className={styles.muted}>Your latest submission is being reviewed by our team.</p>
              <DocumentList documents={application?.documents ?? []} />
            </>
          )}

          {!isLoading && !error && status === "Rejected" && (
            <>
              <p className={styles.muted}>Your latest submission needs changes before verification can be completed.</p>
              <DocumentList documents={application?.documents ?? []} />
              <button type="button" className={styles.secondaryBtn} onClick={() => navigate("/fica-verification")}>
                Update documents
              </button>
            </>
          )}

          {!isLoading && !error && status === "NotSubmitted" && (
            <>
              <p className={styles.muted}>Submit your FICA documents to complete verification.</p>
              <button type="button" className={styles.primaryBtn} onClick={() => navigate("/fica-verification")}>
                Start verification
              </button>
            </>
          )}
        </section>
      )}
    </main>
  );
}

function InfoItem({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className={styles.infoItem}>
      <span>{label}</span>
      <strong className={mono ? styles.mono : undefined}>{value}</strong>
    </div>
  );
}

function DocumentList({ documents }: { documents: KycDocumentStatus[] }) {
  if (!documents.length) {
    return <p className={styles.muted}>No documents are attached to this submission.</p>;
  }

  return (
    <div className={styles.documentList}>
      {documents.map((document) => (
        <div className={styles.documentRow} key={`${document.documentType}-${document.fileName}`}>
          <div>
            <strong>{labels[document.documentType]}</strong>
            <span>{document.fileName}</span>
          </div>
          <span className={`${styles.statusPill} ${statusClass(document.status)}`}>{document.status}</span>
        </div>
      ))}
    </div>
  );
}
