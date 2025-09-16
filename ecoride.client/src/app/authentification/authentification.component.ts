import { Component } from '@angular/core';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-authentification',
  standalone: false,
  templateUrl: './authentification.component.html',
  styleUrl: './authentification.component.css'
})
export class AuthentificationComponent {
  email = '';
  password = '';
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) { }

  login() {
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.router.navigate(['/accueil']); // Ou une page protégée
      },
      error: (err) => {
        this.errorMessage = 'Email ou mot de passe incorrect.';
        console.error('Erreur de connexion:', err);
      }
    });
  }
}
