import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ShellComponent } from './layout/shell/shell.component';
import { authGuard, roleGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule)
      },
      {
        path: 'documents',
        loadChildren: () => import('./features/documents/documents.module').then(m => m.DocumentsModule)
      },
      {
        path: 'chat',
        loadChildren: () => import('./features/chat/chat.module').then(m => m.ChatModule)
      },
      {
        path: 'compare',
        loadChildren: () => import('./features/compare/compare.module').then(m => m.CompareModule)
      },
      {
        path: 'summary',
        loadChildren: () => import('./features/summary/summary.module').then(m => m.SummaryModule)
      },
      {
        path: 'analytics',
        canActivate: [roleGuard(['Admin', 'Manager'])],
        loadChildren: () => import('./features/analytics/analytics.module').then(m => m.AnalyticsModule)
      },
      {
        path: 'users',
        canActivate: [roleGuard(['Admin'])],
        loadChildren: () => import('./features/users/users.module').then(m => m.UsersModule)
      },
      {
        path: 'audit',
        canActivate: [roleGuard(['Admin', 'Auditor'])],
        loadChildren: () => import('./features/audit/audit.module').then(m => m.AuditModule)
      },
      {
        path: 'settings',
        loadChildren: () => import('./features/settings/settings.module').then(m => m.SettingsModule)
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
