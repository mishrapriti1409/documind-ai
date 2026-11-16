import { Component, OnInit } from '@angular/core';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { User } from '../../core/models';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
  users: User[] = [];
  filteredUsers: User[] = [];
  searchTerm: string = '';
  roleFilter: string = '';
  isLoading = true;
  errorMessage = '';
  successMessage = '';
  currentUserId: string = '';

  readonly roles = ['Admin', 'Manager', 'Employee', 'Auditor'];

  constructor(
    private userService: UserService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.currentUserValue?.id || '';
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.userService.getUsers().subscribe({
      next: (users: User[]) => {
        this.users = users;
        this.applyFilter();
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Failed to load organization members.';
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    const term = this.searchTerm.toLowerCase().trim();
    this.filteredUsers = this.users.filter(u => {
      const matchesTerm = !term ||
        u.firstName.toLowerCase().includes(term) ||
        u.lastName.toLowerCase().includes(term) ||
        u.email.toLowerCase().includes(term);

      const matchesRole = !this.roleFilter || u.role === this.roleFilter;

      return matchesTerm && matchesRole;
    });
  }

  onRoleChange(user: User, newRole: string): void {
    if (user.role === newRole) return;

    this.userService.updateRole(user.id, newRole).subscribe({
      next: () => {
        user.role = newRole as any;
        this.successMessage = `Updated ${user.firstName}'s role to ${newRole}.`;
        setTimeout(() => (this.successMessage = ''), 3000);
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Failed to update user role.';
      }
    });
  }

  toggleActive(user: User): void {
    if (user.id === this.currentUserId) {
      alert('You cannot deactivate your own account.');
      return;
    }

    const action = user.isActive ? 'deactivate' : 'activate';
    if (!confirm(`Are you sure you want to ${action} ${user.firstName} ${user.lastName}?`)) {
      return;
    }

    this.userService.toggleActive(user.id).subscribe({
      next: () => {
        user.isActive = !user.isActive;
        this.successMessage = `Successfully ${action}d ${user.firstName} ${user.lastName}.`;
        setTimeout(() => (this.successMessage = ''), 3000);
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || `Failed to ${action} user.`;
      }
    });
  }
}
