export type ExpenseCategory = 'Transport' | 'Accommodation' | 'Food' | 'Tickets' | 'Shopping' | 'Other';

export interface Expense {
  id: number;
  title: string;
  category: ExpenseCategory;
  amount: number;
  date: string;
  description?: string;
  travelPlanId: number;
  userId: number;
}

export interface CreateExpense {
  title: string;
  category: ExpenseCategory;
  amount: number;
  date: string;
  description?: string;
}

export interface UpdateExpense {
  title?: string;
  category?: ExpenseCategory;
  amount?: number;
  date?: string;
  description?: string;
}

export interface BudgetSummary {
  travelPlanId: number;
  plannedBudget: number;
  totalExpenses: number;
  remainingBudget: number;
  expensesByCategory: Record<string, number>;
}
