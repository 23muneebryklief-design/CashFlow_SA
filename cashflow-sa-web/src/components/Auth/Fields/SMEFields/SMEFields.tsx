import { AuthInput, AuthSelect } from "../../Ui";

interface SMEFieldsProps {
  companyName: string;
  contactPerson: string;
  companyEmail: string;
  companyPhoneNumber: string;
  registrationNumber: string;
  taxNumber: string;
  industry: string;

  setCompanyName: (value: string) => void;
  setContactPerson: (value: string) => void;
  setCompanyEmail: (value: string) => void;
  setCompanyPhoneNumber: (value: string) => void;
  setRegistrationNumber: (value: string) => void;
  setTaxNumber: (value: string) => void;
  setIndustry: (value: string) => void;

  errors?: {
    companyName?: string;
    contactPerson?: string;
    companyEmail?: string;
    companyPhoneNumber?: string;
    registrationNumber?: string;
    taxNumber?: string;
    industry?: string;
  };
}

export default function SMEFields({
  companyName,
  contactPerson,
  companyEmail,
  companyPhoneNumber,
  registrationNumber,
  taxNumber,
  industry,
  setCompanyName,
  setContactPerson,
  setCompanyEmail,
  setCompanyPhoneNumber,
  setRegistrationNumber,
  setTaxNumber,
  setIndustry,
  errors,
}: SMEFieldsProps) {
  return (
    <>
      <AuthInput
        label="Company Name"
        value={companyName}
        onChange={setCompanyName}
        required
        error={errors?.companyName}
      />

      <AuthInput
        label="Contact Person"
        value={contactPerson}
        onChange={setContactPerson}
        required
        error={errors?.contactPerson}
      />

      <AuthInput
        label="Company Email"
        type="email"
        value={companyEmail}
        onChange={setCompanyEmail}
        required
        error={errors?.companyEmail}
      />

      <AuthInput
        label="Company Phone Number"
        value={companyPhoneNumber}
        onChange={setCompanyPhoneNumber}
        required
        error={errors?.companyPhoneNumber}
      />

      <AuthInput
        label="Registration Number"
        value={registrationNumber}
        onChange={setRegistrationNumber}
        required
        error={errors?.registrationNumber}
      />

      <AuthInput
        label="Tax Number"
        value={taxNumber}
        onChange={setTaxNumber}
        required
        error={errors?.taxNumber}
      />

      <AuthSelect
        label="Industry"
        value={industry}
        onChange={setIndustry}
        required
        error={errors?.industry}
        options={[
          { value: "Agriculture", label: "Agriculture" },
          { value: "Technology", label: "Technology" },
          { value: "Manufacturing", label: "Manufacturing" },
          { value: "Retail", label: "Retail" },
          { value: "Finance", label: "Finance" },
          { value: "Healthcare", label: "Healthcare" },
          { value: "Construction", label: "Construction" },
          { value: "Education", label: "Education" },
          { value: "Other", label: "Other" },
        ]}
      />
    </>
  );
}