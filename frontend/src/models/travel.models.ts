export interface TravelPlan {
  id: number;
  title: string;
  description?: string;
  startDate: string;
  endDate: string;
  budget: number;
  notes?: string;
  userId: number;
  createdAt: string;
  updatedAt: string;
  destinations: Destination[];
  activities: Activity[];
  checklistItems: ChecklistItem[];
}

export interface CreateTravelPlan {
  title: string;
  description?: string;
  startDate: string;
  endDate: string;
  budget: number;
  notes?: string;
}

export interface UpdateTravelPlan {
  title?: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  budget?: number;
  notes?: string;
}

export interface Destination {
  id: number;
  name: string;
  location: string;
  arrivalDate: string;
  departureDate: string;
  description?: string;
  notes?: string;
  travelPlanId: number;
}

export interface CreateDestination {
  name: string;
  location: string;
  arrivalDate: string;
  departureDate: string;
  description?: string;
  notes?: string;
}

export interface UpdateDestination {
  name?: string;
  location?: string;
  arrivalDate?: string;
  departureDate?: string;
  description?: string;
  notes?: string;
}

export type ActivityStatus = 'Planned' | 'Reserved' | 'Completed' | 'Cancelled';

export interface Activity {
  id: number;
  title: string;
  date: string;
  time?: string;
  location?: string;
  description?: string;
  estimatedCost?: number;
  status: ActivityStatus;
  travelPlanId: number;
  destinationId?: number;
}

export interface CreateActivity {
  title: string;
  date: string;
  time?: string;
  location?: string;
  description?: string;
  estimatedCost?: number;
  status: ActivityStatus;
  destinationId?: number;
}

export interface UpdateActivity {
  title?: string;
  date?: string;
  time?: string;
  location?: string;
  description?: string;
  estimatedCost?: number;
  status?: ActivityStatus;
  destinationId?: number;
}

export interface ChecklistItem {
  id: number;
  title: string;
  isCompleted: boolean;
  travelPlanId: number;
  createdAt: string;
}

export interface CreateChecklistItem {
  title: string;
}

export interface UpdateChecklistItem {
  title?: string;
  isCompleted?: boolean;
}
