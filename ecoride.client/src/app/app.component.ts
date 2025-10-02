import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
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

  

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  title = 'ecoride.client';
}
