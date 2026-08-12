import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../Hooks/useAuth";
import { useKycStatus } from "../../Hooks/useKycStatus";
import {
  submitKycApplication,
  uploadKycDocument,
  validateKycFile,
  type DocumentType,
  type KycDocumentInput,
  type KycDocumentStatus,
} from "../../Services/kycService";
import styles from "./FicaVerification.module.css";

interface RequiredDoc {
  type: DocumentType;
  label: string;
  helpText: string;
}

const REQUIRED_DOCS: RequiredDoc[] = [
  {
    type: "CompanyRegistration",
    label: "Company registration document",
    helpText: "CIPC registration certificate (CoR 14.3 / CoR 14.1).",
  },
  {
    type: "IdentityDocument",
    label: "Director / owner ID document",
    helpText: "A South African ID or valid passport.",
  },
  {
    type: "ProofOfAddress",
    label: "Proof of business address",
    helpText: "Utility bill or bank statement, dated within the last 3 months.",
  },
  {
    type: "TaxCertificate",
    label: "Tax clearance certificate",
    helpText: "SARS tax compliance status / clearance certificate.",
  },
];

type UploadSlotState =
  | { state: "empty" }
  | { state: "uploading"; fileName: string }
  | { state: "uploaded"; fileName: string; document: KycDocumentInput }
  | { state: "server"; fileName: string; status: KycDocumentStatus["status"] }
  | { state: "error"; fileName: string; message: string };

export default function FicaVerification() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { status, application, isLoading, error, refetch } = useKycStatus();

  const [slots, setSlots] = useState<Record<DocumentType, UploadSlotState>>({
    CompanyRegistration: { state: "empty" },
    IdentityDocument: { state: "empty" },
    ProofOfAddress: { state: "empty" },
    TaxCertificate: { state: "empty" },
    BankStatement: { state: "empty" },
    Other: { state: "empty" },
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    if (!application?.documents) return;

    setSlots((prev) => {
      const next = { ...prev };

      for (const document of application.documents) {
        if (document.documentType in next && next[document.documentType].state === "empty") {
          next[document.documentType] = {
            state: "server",
            fileName: document.fileName,
            status: document.status,
          };
        }
      }

      return next;
    });
  }, [application?.applicationId, application?.documents]);


  async function handleFileSelect(docType: DocumentType, file: File | undefined) {
    if (!file) return;

    const validationError = validateKycFile(file);
    if (validationError) {
      setSlots((prev) => ({
        ...prev,
        [docType]: { state: "error", fileName: file.name, message: validationError },
      }));
      return;
    }

    setSlots((prev) => ({ ...prev, [docType]: { state: "uploading", fileName: file.name } }));

    try {
      const uploaded = await uploadKycDocument(file);
      setSlots((prev) => ({
        ...prev,
        [docType]: {
          state: "uploaded",
          fileName: file.name,
          document: {
            documentType: docType,
            fileName: uploaded.fileName,
            filePath: uploaded.filePath,
            fileSize: uploaded.fileSize,
          },
        },
      }));
    } catch {
      setSlots((prev) => ({
        ...prev,
        [docType]: { state: "error", fileName: file.name, message: "Upload failed. Please try again." },
      }));
    }
  }

  const allRequiredUploaded = REQUIRED_DOCS.every((doc) =>
    slots[doc.type].state === "uploaded"
  );

  async function handleSubmit() {
    if (!user?.profileId || !allRequiredUploaded) return;

    setIsSubmitting(true);
    setSubmitError(null);

    const documents: KycDocumentInput[] = Object.values(slots)
      .filter((slot): slot is Extract<UploadSlotState, { state: "uploaded" }> => slot.state === "uploaded")
      .map((slot) => slot.document);

    try {
      await submitKycApplication(user.profileId, documents);
      refetch();
    } catch {
      setSubmitError("Could not submit your application. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isLoading) {
    return (
      <main className={styles.page}>
        <p className={styles.status}>Checking your FICA verification status...</p>
      </main>
    );
  }

  if (error) {
    return (
      <main className={styles.page}>
        <p className={styles.status}>{error}</p>
      </main>
    );
  }

  const submittedDocuments = (
    <section className={styles.docList}>
      {REQUIRED_DOCS.map((doc) => {
        const slot = slots[doc.type];
        return (
          <div key={doc.type} className={styles.docRow}>
            <div className={styles.docInfo}>
              <span className={styles.docLabel}>{doc.label}</span>
              <span className={styles.docHelp}>{doc.helpText}</span>
            </div>
            <div className={styles.docAction}>
              {slot.state === "server" ? (
                <div className={styles.docUploaded}>
                  <span className={`${styles.statusBadge} ${styles[`status${slot.status}`]}`}>
                    {slot.status}
                  </span>
                  <span className={styles.docFileName}>{slot.fileName}</span>
                </div>
              ) : (
                <span className={styles.docStatusText}>Submitted</span>
              )}
            </div>
          </div>
        );
      })}
    </section>
  );

  // Verified SMEs shouldn't normally land here (the sidebar link disappears),
  // but handle a direct visit gracefully rather than showing an upload form.
  if (status === "Verified") {
    return (
      <main className={styles.page}>
        <header className={styles.header}>
          <p className={styles.eyebrow}>FICA Verification</p>
          <h1>You're verified</h1>
        </header>
        <section className={styles.banner + " " + styles.bannerVerified}>
          <p>Your FICA verification is complete. There's nothing further needed here.</p>
          <button type="button" className={styles.linkBtn} onClick={() => navigate("/profile")}>
            View in Profile
          </button>
        </section>
        {submittedDocuments}
      </main>
    );
  }

  if (status === "Pending") {
    return (
      <main className={styles.page}>
        <header className={styles.header}>
          <p className={styles.eyebrow}>FICA Verification</p>
          <h1>Application under review</h1>
        </header>
        <section className={styles.banner + " " + styles.bannerPending}>
          <p>
            Your documents were submitted on{" "}
            {application ? new Date(application.applicationDate).toLocaleDateString("en-ZA") : "—"} and are
            currently being reviewed by our team. We'll update your status here once a decision is made.
          </p>
        </section>
        {submittedDocuments}
      </main>
    );
  }

  const isRejected = status === "Rejected";

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>FICA Verification</p>
        <h1>{isRejected ? "Resubmit your documents" : "Verify your business"}</h1>
        <p className={styles.subhead}>
          {isRejected
            ? "Your previous submission wasn't approved. Upload corrected documents below to resubmit."
            : "Upload the documents below to unlock invoice uploads and funding requests."}
        </p>
      </header>

      {isRejected && (
        <section className={styles.banner + " " + styles.bannerRejected}>
          <p>Your last application was rejected. Please check each document is clear, current, and matches the request above before resubmitting.</p>
        </section>
      )}

      <section className={styles.docList}>
        {REQUIRED_DOCS.map((doc) => {
          const slot = slots[doc.type];
          return (
            <div key={doc.type} className={styles.docRow}>
              <div className={styles.docInfo}>
                <span className={styles.docLabel}>{doc.label}</span>
                <span className={styles.docHelp}>{doc.helpText}</span>
              </div>

              <div className={styles.docAction}>
                {slot.state === "empty" && (
                  <label className={styles.uploadBtn}>
                    Upload
                    <input
                      type="file"
                      accept=".pdf,.jpg,.jpeg,.png"
                      className={styles.hiddenInput}
                      onChange={(e) => handleFileSelect(doc.type, e.target.files?.[0])}
                    />
                  </label>
                )}

                {slot.state === "uploading" && <span className={styles.docStatusText}>Uploading...</span>}

                {slot.state === "server" && (
                  <div className={styles.docUploaded}>
                    <span className={`${styles.statusBadge} ${styles[`status${slot.status}`]}`}>
                      {slot.status}
                    </span>
                    <span className={styles.docFileName}>{slot.fileName}</span>
                    {slot.status === "Rejected" && (
                      <label className={styles.replaceBtn}>
                        Replace
                        <input
                          type="file"
                          accept=".pdf,.jpg,.jpeg,.png"
                          className={styles.hiddenInput}
                          onChange={(e) => handleFileSelect(doc.type, e.target.files?.[0])}
                        />
                      </label>
                    )}
                  </div>
                )}

                {slot.state === "uploaded" && (
                  <div className={styles.docUploaded}>
                    <span className={styles.checkmark}>✓</span>
                    <span className={styles.docFileName}>{slot.fileName}</span>
                    <label className={styles.replaceBtn}>
                      Replace
                      <input
                        type="file"
                        accept=".pdf,.jpg,.jpeg,.png"
                        className={styles.hiddenInput}
                        onChange={(e) => handleFileSelect(doc.type, e.target.files?.[0])}
                      />
                    </label>
                  </div>
                )}

                {slot.state === "error" && (
                  <div className={styles.docError}>
                    <span>{slot.message}</span>
                    <label className={styles.uploadBtn}>
                      Try again
                      <input
                        type="file"
                        accept=".pdf,.jpg,.jpeg,.png"
                        className={styles.hiddenInput}
                        onChange={(e) => handleFileSelect(doc.type, e.target.files?.[0])}
                      />
                    </label>
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </section>

      {submitError && <p className={styles.submitError}>{submitError}</p>}

      <button
        type="button"
        className={styles.submitBtn}
        disabled={!allRequiredUploaded || isSubmitting}
        onClick={handleSubmit}
      >
        {isSubmitting ? "Submitting..." : isRejected ? "Resubmit for Verification" : "Submit for Verification"}
      </button>
    </main>
  );
}
