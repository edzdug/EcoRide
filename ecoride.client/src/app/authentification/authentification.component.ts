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
        // Lire le message d'erreur envoyé par l'API
        if (err.error && typeof err.error === 'string') {
          this.errorMessage = err.error;
        } else {
          this.errorMessage = 'Erreur de connexion. Veuillez réessayer.';
        }

        console.error('Erreur de connexion:', err);
      }
    });
  }
}
