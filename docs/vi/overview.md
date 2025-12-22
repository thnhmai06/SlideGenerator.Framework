# Tổng quan Framework

## Mục lục

1. [Mục đích](#mục-đích)
2. [Module](#module)
3. [Ràng buộc chính](#ràng-buộc-chính)
4. [Luồng và dispose](#luồng-và-dispose)

## Mục đích

`SlideGenerator.Framework` cung cấp engine xử lý cốt lõi cho:

- Đọc workbook và worksheet
- Nạp template slide và clone
- Thay thế text
- Thay thế ảnh (ROI + crop + resize)
- Resolve cloud URL

Backend chỉ điều phối và I/O; framework thực thi xử lý.

## Module

- `Cloud`: resolve link cloud sang link tải trực tiếp.
- `Sheet`: truy cập workbook/worksheet cho Excel/CSV.
- `Slide`: nạp template, clone slide, thay text/ảnh.
- `Image`: phát hiện ROI, crop, resize.

## Ràng buộc chính

- Dùng API của framework cho workbook/slide/image/cloud.
- Không re‑implement logic ở backend.
- Không thay thế bằng thư viện thứ ba.

## Luồng và dispose

- `Workbook` và các model presentation là `IDisposable`.
- Luôn dispose để tránh khóa file.
