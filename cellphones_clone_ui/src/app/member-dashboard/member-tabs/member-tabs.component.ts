import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PurchaseHistoryComponent } from '../purchase-history/purchase-history.component';
import { WarrantyComponent } from '../warranty/warranty.component';
import { InforComponent } from '../infor/infor.component';

interface SidebarItem {
    id: string;
    label: string;
    icon: string;
}

@Component({
    selector: 'app-member-tabs',
    standalone: true,
    imports: [CommonModule, PurchaseHistoryComponent, WarrantyComponent, InforComponent],
    templateUrl: './member-tabs.component.html',
    styleUrls: ['./member-tabs.component.scss']
})
export class MemberTabsComponent {
    activeTab = signal<string>('overview');

    sidebarItems: SidebarItem[] = [
        {
            id: 'overview',
            label: 'Tổng quan',
            icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6'
        },
        {
            id: 'orders',
            label: 'Lịch sử mua hàng',
            icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2'
        },
        {
            id: 'warranty',
            label: 'Tra cứu bảo hành',
            icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z'
        },
        {
            id: 'membership',
            label: 'Hạng thành viên và ưu đãi',
            icon: 'M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z'
        },
        {
            id: 'offers',
            label: 'Ưu đãi và đơn hàng S-Business',
            icon: 'M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z'
        },
        {
            id: 'student',
            label: 'Ưu đãi S-Student và S-Teacher',
            icon: 'M12 14l9-5-9-5-9 5 9 5z' // Simplified Cap
            // Or 'M12 14l9-5-9-5-9 5 9 5zm0 0l6.16-3.422a12.083 12.083 0 01.665 6.479A11.952 11.952 0 0012 20.055a11.952 11.952 0 00-6.824-2.998 12.078 12.078 0 01.665-6.479L12 14zm-4 6v-7.5l4-2.222'
        },
        {
            id: 'account',
            label: 'Thông tin tài khoản',
            icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z'
        },
        {
            id: 'store',
            label: 'Tìm kiếm cửa hàng',
            icon: 'M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z'
        },
        {
            id: 'policy',
            label: 'Chính sách bảo hành',
            icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
        },
        {
            id: 'feedback',
            label: 'Góp ý - Phản hồi - Hỗ trợ',
            icon: 'M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z'
        },
        {
            id: 'terms',
            label: 'Điều khoản sử dụng',
            icon: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253'
        },
        {
            id: 'logout',
            label: 'Đăng xuất',
            icon: 'M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1'
        }
    ];

    setActiveTab(tabId: string) {
        this.activeTab.set(tabId);
    }
}
