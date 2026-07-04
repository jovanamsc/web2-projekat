import api from './api';
import type {
  TravelPlan, CreateTravelPlan, UpdateTravelPlan,
  Destination, CreateDestination, UpdateDestination,
  Activity, CreateActivity, UpdateActivity,
  ChecklistItem, CreateChecklistItem, UpdateChecklistItem,
} from '../models/travel.models';
import type { ShareLink, CreateShareLink } from '../models/share.models';

const travelService = {
  // Planovi putovanja
  async getAll(): Promise<TravelPlan[]> {
    const response = await api.get<TravelPlan[]>('/api/travel-plans');
    return response.data;
  },

  async getById(id: number): Promise<TravelPlan> {
    const response = await api.get<TravelPlan>(`/api/travel-plans/${id}`);
    return response.data;
  },

  async create(data: CreateTravelPlan): Promise<TravelPlan> {
    const response = await api.post<TravelPlan>('/api/travel-plans', data);
    return response.data;
  },

  async update(id: number, data: UpdateTravelPlan): Promise<TravelPlan> {
    const response = await api.put<TravelPlan>(`/api/travel-plans/${id}`, data);
    return response.data;
  },

  async remove(id: number): Promise<void> {
    await api.delete(`/api/travel-plans/${id}`);
  },

  // Destinacije
  async getDestinations(planId: number): Promise<Destination[]> {
    const response = await api.get<Destination[]>(`/api/travel-plans/${planId}/destinations`);
    return response.data;
  },

  async createDestination(planId: number, data: CreateDestination): Promise<Destination> {
    const response = await api.post<Destination>(`/api/travel-plans/${planId}/destinations`, data);
    return response.data;
  },

  async updateDestination(planId: number, id: number, data: UpdateDestination): Promise<Destination> {
    const response = await api.put<Destination>(`/api/travel-plans/${planId}/destinations/${id}`, data);
    return response.data;
  },

  async deleteDestination(planId: number, id: number): Promise<void> {
    await api.delete(`/api/travel-plans/${planId}/destinations/${id}`);
  },

  // Aktivnosti
  async getActivities(planId: number): Promise<Activity[]> {
    const response = await api.get<Activity[]>(`/api/travel-plans/${planId}/activities`);
    return response.data;
  },

  async createActivity(planId: number, data: CreateActivity): Promise<Activity> {
    const response = await api.post<Activity>(`/api/travel-plans/${planId}/activities`, data);
    return response.data;
  },

  async updateActivity(planId: number, id: number, data: UpdateActivity): Promise<Activity> {
    const response = await api.put<Activity>(`/api/travel-plans/${planId}/activities/${id}`, data);
    return response.data;
  },

  async deleteActivity(planId: number, id: number): Promise<void> {
    await api.delete(`/api/travel-plans/${planId}/activities/${id}`);
  },

  // Ceklista
  async getChecklist(planId: number): Promise<ChecklistItem[]> {
    const response = await api.get<ChecklistItem[]>(`/api/travel-plans/${planId}/checklist`);
    return response.data;
  },

  async createChecklistItem(planId: number, data: CreateChecklistItem): Promise<ChecklistItem> {
    const response = await api.post<ChecklistItem>(`/api/travel-plans/${planId}/checklist`, data);
    return response.data;
  },

  async updateChecklistItem(planId: number, id: number, data: UpdateChecklistItem): Promise<ChecklistItem> {
    const response = await api.put<ChecklistItem>(`/api/travel-plans/${planId}/checklist/${id}`, data);
    return response.data;
  },

  async deleteChecklistItem(planId: number, id: number): Promise<void> {
    await api.delete(`/api/travel-plans/${planId}/checklist/${id}`);
  },

  // Dijeljenje
  async createShareLink(planId: number, data: CreateShareLink): Promise<ShareLink> {
    const response = await api.post<ShareLink>(`/api/travel-plans/${planId}/share`, data);
    return response.data;
  },

  async getShareLinks(planId: number): Promise<ShareLink[]> {
    const response = await api.get<ShareLink[]>(`/api/travel-plans/${planId}/share`);
    return response.data;
  },

  async deleteShareLink(planId: number, token: string): Promise<void> {
    await api.delete(`/api/travel-plans/${planId}/share/${token}`);
  },

  async getSharedPlan(token: string): Promise<TravelPlan> {
    const response = await api.get<TravelPlan>(`/api/travel-plans/shared/${token}`);
    return response.data;
  },

  async getShareLinkInfo(token: string): Promise<ShareLink> {
    const response = await api.get<ShareLink>(`/api/travel-plans/shared/${token}/info`);
    return response.data;
  },
};

export default travelService;
