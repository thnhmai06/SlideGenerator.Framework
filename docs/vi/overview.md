# Tổng quan Framework

Phiên bản tiếng Anh: [English](../en/overview.md)

## Mục lục

- [Tổng quan Framework](#tổng-quan-framework)
  - [Mục lục](#mục-lục)
  - [Mục đích](#mục-đích)
  - [Module](#module)
  - [Luồng và dispose](#luồng-và-dispose)

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
- `Image`: phát hiện ROI, crop, resize và các tiện ích liên quan.

## Luồng và dispose

- `Workbook` và các model presentation là `IDisposable`.
- Luôn dispose khi xong để tránh khóa file.
