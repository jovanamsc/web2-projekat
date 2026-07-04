export type AccessLevel = 'VIEW' | 'EDIT';

export interface ShareLink {
  travelPlanId: number;
  token: string;
  accessLevel: AccessLevel;
  createdAt: string;
  expiresAt?: string;
}

export interface CreateShareLink {
  accessLevel: AccessLevel;
  expiryDays: number;
}
