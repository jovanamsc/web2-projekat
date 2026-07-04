import api from './api';
import type { BudgetSummary, CreateExpense, Expense, UpdateExpense } from '../models/expense.models';

const expenseService = {
  async getExpenses(planId: number): Promise<Expense[]> {
    const response = await api.get<Expense[]>(`/api/travel-plans/${planId}/expenses`);
    return response.data;
  },

  async createExpense(planId: number, data: CreateExpense): Promise<Expense> {
    const response = await api.post<Expense>(`/api/travel-plans/${planId}/expenses`, data);
    return response.data;
  },

  async updateExpense(planId: number, id: number, data: UpdateExpense): Promise<Expense> {
    const response = await api.put<Expense>(`/api/travel-plans/${planId}/expenses/${id}`, data);
    return response.data;
  },

  async deleteExpense(planId: number, id: number): Promise<void> {
    await api.delete(`/api/travel-plans/${planId}/expenses/${id}`);
  },

  async getBudgetSummary(planId: number, plannedBudget: number): Promise<BudgetSummary> {
    const response = await api.get<BudgetSummary>(
      `/api/travel-plans/${planId}/expenses/summary?plannedBudget=${plannedBudget}`
    );
    return response.data;
  },
};

export default expenseService;
