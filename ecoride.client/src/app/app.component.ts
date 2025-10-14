import { HttpClient } from '@angular/common/http';
import { Component, HostListener, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService }from './authentification/auth.service';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];
  constructor(private http: HttpClient, public authService: AuthService, private routes: ActivatedRoute, private router: Router) {}

  ngOnInit() {}

  menuOpen = false;

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event) {
    const target = event.target as HTMLElement;
    const isMenu = target.closest('.nav-links');
    const isButton = target.closest('.menu-toggle');

    if (!isMenu && !isButton) {
      this.menuOpen = false;
    }
  }

  closeMenu() {
    this.menuOpen = false;
  }

  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
    this.closeMenu();
  }

  title = 'ecoride.client';
}
