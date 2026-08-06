import { AuthSelect } from "../../Ui";

interface InvestorFieldsProps {
  riskAppetite: string;
  setRiskAppetite: (value: string) => void;
  errors?: {
    riskAppetite?: string;
  };
}

export default function InvestorFields({
  riskAppetite,
  setRiskAppetite,
  errors,
}: InvestorFieldsProps) {
  return (
    <AuthSelect
      label="Risk Appetite"
      value={riskAppetite}
      onChange={setRiskAppetite}
      required
      error={errors?.riskAppetite}
      options={[
        { value: "Low", label: "Low" },
        { value: "Medium", label: "Medium" },
        { value: "High", label: "High" },
      ]}
    />
  );
}