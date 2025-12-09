export interface User {
  userId: number;
  fullName: string;
  token: string;
  email: string;
  password: string;
  companyId: number;
  departmentId: number;
  jobId: number;
  title: string;
  nationality: string;
  identityNumber: string;
  mobile: string;
  address: string;
  qualification: string;
  isActive: boolean;
  roles: [];
  permissions: [];
}
